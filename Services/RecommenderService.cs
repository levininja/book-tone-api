using BookToneApi.Models;
using System.Text.Json;
using System.Net.Http;

namespace BookToneApi.Services
{
    public class RecommenderService : IRecommenderService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RecommenderService> _logger;
        private readonly IHardcoverApiService _hardcoverApiService;
        private readonly string _ollamaUrl;

        public RecommenderService(ILogger<RecommenderService> logger, HttpClient httpClient, IHardcoverApiService hardcoverApiService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _hardcoverApiService = hardcoverApiService;
            _ollamaUrl = "http://localhost:11434"; // Default Ollama URL
        }

        public async Task<BookToneRecommendationResponseDto> GetRecommendationAsync(string bookTitle, string bookAuthor, List<string> genres)
        {
            try
            {
                List<string> tones = await GenerateToneRecommendationsAsync(bookTitle, bookAuthor, genres);
                
                return new BookToneRecommendationResponseDto
                {
                    Id = 0, // Will be set by the database
                    BookTitle = bookTitle,
                    BookAuthor = bookAuthor,
                    Tones = tones,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendation for {BookTitle} by {BookAuthor}", bookTitle, bookAuthor);
                throw;
            }
        }

        private async Task<List<string>> GenerateToneRecommendationsAsync(string bookTitle, string bookAuthor, List<string> genres)
        {
            try
            {
                // Get mood tags from Hardcover API
                var moodTags = await _hardcoverApiService.GetMoodTagsAsync(bookTitle, bookAuthor);
                
                // Create prompt for Phi model with mood tags
                string prompt = CreatePrompt(bookTitle, bookAuthor, genres, moodTags);
                
                // Call Ollama API
                var response = await CallOllamaAsync(prompt);
                
                if (!string.IsNullOrEmpty(response))
                {
                    // Process the response to extract tones
                    List<string> tones = ProcessOllamaResponse(response);
                    if (tones.Count > 0)
                    {
                        return tones;
                    }
                }
                
                // Return empty list if Ollama fails or returns empty
                _logger.LogWarning("Ollama response was empty or invalid. Returning empty tone list.");
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Ollama API. Returning empty tone list.");
                return new List<string>();
            }
        }

        private string CreatePrompt(string bookTitle, string bookAuthor, List<string> genres, List<string> moodTags)
        {
            string genreList = string.Join(", ", genres);
            string moodTagsList = moodTags.Count > 0 ? string.Join(", ", moodTags) : "none available";
            
            return $@"Based on the book '{bookTitle}' by {bookAuthor} in the genres: {genreList}, and with mood tags from readers: {moodTagsList}, what would be the 3 most appropriate tones for this book? 

Choose from these tones with their definitions:

Poignant: Evokes a deep emotional response, such as beauty, sadness, longing, or emotional catharsis.
Melancholic: Wistful sadness, emotional heaviness.
Bittersweet: Happy-sad blend; joy tinged with sorrow.
Gut-wrenching: Overwhelming grief, devastation, or emotional agony.
Heartwarming: Deeply joyful, comforting, emotionally uplifting.
Haunting: Emotionally lingering; either mournful or eerie.
Dark: Bleak, grim, disturbing, negative emotional or thematic tone.
Bleak: Hopeless, emotionally barren, devoid of comfort.
Gritty: Harsh realism; physical or moral ugliness.
Cynical: Distrustful of humanity, institutions, or motives.
Unsettling: Subtly discomforting; leaves the reader emotionally off balance.
Hard-boiled: Cynical, terse, emotionally detached tone, often in crime or noir fiction.
Grimdark: Bleak, gritty, cynical, and devoid of moral clarity.
Disturbing: Emotionally jarring or morally challenging; evokes discomfort through realistic trauma, psychological violation, or taboo-breaking content. Often slow-burn or quiet in tone.
Horrific: Evokes visceral fear, revulsion, or existential dread. It emphasizes the grotesque, the terrifying, or the emotionally overwhelming through graphic, sensory, or psychologically disturbing content.
Macabre: Focused on death, decay, and mortality.
Grotesque: Distorted, deformed, or unsettling in physical, thematic, or psychological ways.
Claustrophobic: Oppressive, trapped, suffocating, whether emotionally or physically.
Intense: High emotional pressure, tension, or stress, whether internal or external.
Suspenseful: Driven by tension, uncertainty, and anticipation.
Atmospheric: Immersive mood and sensory presence.
Lyrical: Beautiful, poetic, stylized language that drives the emotional tone.
Surreal: Dreamlike, bizarre, reality-bending tone.
Mystical: Numinous, transcendent, spiritual, or magical in tone.
Dramatic: High emotional stakes, emotional display, conflict, and heightened energy.
Heroic: Dramatization of courage, valor, sacrifice, grand quests.
Tragic: Dramatization of downfall, loss, failure, sorrow.
Romantic: Focused on love, passion, and relationship-driven emotional tone.
Steamy: Erotic, sensual, sexually charged tone.
Sweet: Wholesome, tender, gentle, and emotionally innocent romance.
Angsty: Emotional turmoil, yearning, heartbreak, tension before resolution.
Flirty: Playful, teasing, often lighthearted romantic tension.
Realistic: Grounded in plausibility, the mundane, the ordinary; downplays melodrama or artifice.
Detached: Emotionally neutral, clinical, observational; understated affect within realism.
Upbeat: Energetic, optimistic, and emotionally bright; maintains a forward-driving tone.
Hopeful: Oriented toward positive change, renewal, or resilience.
Uplifting: Inspires or restores emotionally; leaves the reader feeling elevated.
Playful: Full of fun, mischief, or cleverness; doesn't take itself too seriously.
Comforting: Emotionally safe, familiar, and gentle; soothing rather than challenging.
Cozy: Intimate, small-scale, often centered on community or domestic spaces.
Whimsical: Fanciful and lighthearted, often with quirky charm or childlike wonder.
Philosophical: Focused on intellectual or existential exploration; abstract or contemplative.
Psychological: Focused on internal tension, mind games, emotional instability, or mental complexity.
Epic: Grand in scope, high-stakes, large-scale storytelling.

Consider the mood tags from readers when making your recommendation, as they provide insight into how readers actually experienced the book's emotional tone.

Respond with only the 3 most appropriate tone names separated by commas.";
        }

        private async Task<string> CallOllamaAsync(string prompt)
        {
            try
            {
                var request = new
                {
                    model = "phi",
                    prompt = prompt,
                    stream = false
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent);
                    return ollamaResponse?.Response ?? string.Empty;
                }
                else
                {
                    _logger.LogError("Ollama API returned status code: {StatusCode}", response.StatusCode);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Ollama API");
                return string.Empty;
            }
        }

        private List<string> ProcessOllamaResponse(string response)
        {
            try
            {
                // Clean up the response and extract tone names
                var cleanResponse = response.Trim().ToLower();
                var validTones = new[] { 
                    "Poignant", "Melancholic", "Bittersweet", "Gut-wrenching", "Heartwarming", "Haunting", 
                    "Dark", "Bleak", "Gritty", "Cynical", "Unsettling", "Hard-boiled", "Grimdark", 
                    "Disturbing", "Horrific", "Macabre", "Grotesque", "Claustrophobic", "Intense", 
                    "Suspenseful", "Atmospheric", "Lyrical", "Surreal", "Mystical", "Dramatic", 
                    "Heroic", "Tragic", "Romantic", "Steamy", "Sweet", "Angsty", "Flirty", 
                    "Realistic", "Detached", "Upbeat", "Hopeful", "Uplifting", "Playful", 
                    "Comforting", "Cozy", "Whimsical", "Philosophical", "Psychological", "Epic"
                };
                
                var foundTones = new List<string>();
                
                foreach (var tone in validTones)
                {
                    if (cleanResponse.Contains(tone.ToLower()))
                    {
                        foundTones.Add(tone);
                    }
                }
                
                // If we found tones, return them (up to 3)
                if (foundTones.Count > 0)
                {
                    return foundTones.Take(3).ToList();
                }
                
                // If no tones found, try to parse comma-separated values
                var parts = cleanResponse.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmedPart = part.Trim();
                    var matchingTone = validTones.FirstOrDefault(t => t.ToLower() == trimmedPart);
                    if (matchingTone != null)
                    {
                        foundTones.Add(matchingTone);
                    }
                }
                
                return foundTones.Take(3).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Ollama response: {Response}", response);
                return new List<string>();
            }
        }

        // Ollama API response model
        private class OllamaResponse
        {
            public string Model { get; set; } = string.Empty;
            public string Response { get; set; } = string.Empty;
            public bool Done { get; set; }
        }




    }
} 