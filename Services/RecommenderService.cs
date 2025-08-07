using BookToneApi.Models;
using BookToneApi.Dtos;
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
            
            // Set a shorter timeout for Ollama requests (15 seconds)
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public async Task<List<string>> GetRecommendationsAsync(int bookId)
        {
            _logger.LogInformation("Starting recommendation generation for book ID: {BookId}", bookId);
            
            try
            {
                // TODO: Implement book lookup from database using bookId
                // For now, return placeholder recommendations
                _logger.LogWarning("GetRecommendationsAsync: Book lookup from database not yet implemented");
                
                return new List<string> { "Realistic", "Optimistic", "Mysterious" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations for book ID: {BookId}", bookId);
                throw;
            }
        }

        private async Task<List<string>> GenerateToneRecommendationsAsync(string bookTitle, string bookAuthor, List<string> genres)
        {
            _logger.LogInformation("GenerateToneRecommendationsAsync: Starting for '{BookTitle}' by {BookAuthor}", bookTitle, bookAuthor);
            
            try
            {
                // TODO: Comment out Hardcover API call for debugging Ollama
                _logger.LogInformation("GenerateToneRecommendationsAsync: Hardcover API call commented out for debugging");
                List<string> moodTags = new List<string>(); // Empty list for now
                _logger.LogInformation("GenerateToneRecommendationsAsync: Using empty mood tags list for debugging");
                
                // Create prompt for Phi model with mood tags
                _logger.LogInformation("GenerateToneRecommendationsAsync: Creating AI prompt");
                string prompt = CreatePrompt(bookTitle, bookAuthor, genres, moodTags);
                _logger.LogDebug("GenerateToneRecommendationsAsync: Created prompt: {Prompt}", prompt);
                
                // Call Ollama API
                _logger.LogInformation("GenerateToneRecommendationsAsync: Calling Ollama API");
                string response = await CallOllamaAsync(prompt);
                _logger.LogInformation("GenerateToneRecommendationsAsync: Received Ollama response: {Response}", response);
                
                if (!string.IsNullOrEmpty(response))
                {
                    // Process the response to extract tones
                    _logger.LogInformation("GenerateToneRecommendationsAsync: Processing Ollama response");
                    List<string> tones = ProcessOllamaResponse(response);
                    _logger.LogInformation("GenerateToneRecommendationsAsync: Extracted {ToneCount} tones: {Tones}", 
                        tones.Count, string.Join(", ", tones));
                    
                    if (tones.Count > 0)
                    {
                        _logger.LogInformation("GenerateToneRecommendationsAsync: Successfully generated tones");
                        return tones;
                    }
                }
                
                // Return empty list if Ollama fails or returns empty
                _logger.LogWarning("GenerateToneRecommendationsAsync: Ollama response was empty or invalid. Returning empty tone list.");
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateToneRecommendationsAsync: Error. Returning empty tone list.");
                return new List<string>();
            }
        }

        private string CreatePrompt(string bookTitle, string bookAuthor, List<string> genres, List<string> moodTags)
        {
            string genreList = string.Join(", ", genres);
            string moodTagsList = moodTags.Count > 0 ? string.Join(", ", moodTags) : "none available";
            
            return $@"Based on the book '{bookTitle}' by {bookAuthor} in the genres: {genreList}, and with mood tags from readers: {moodTagsList}, what would be the most appropriate tones for this book?";
        }

        private async Task<string> CallOllamaAsync(string prompt)
        {
            try
            {
                object request = new
                {
                    model = "booktone-phi", // Using custom model with built-in system prompt
                    prompt = prompt, // No need for system prompt in request since it's built into the model
                    stream = false,
                    // Nucleus sampling - only consider words that make up 90% of probability mass
                    top_p = 0.9,
                    // Temperature - lower for more focused, consistent responses
                    temperature = 0.3,
                    // Repeat penalty - prevents repetitive responses
                    repeat_penalty = 1.1,
                    // Stop sequences - stop at JSON end and newlines for cleaner responses
                    stop = new[] { "]", "\n", "User:", "Assistant:", "System:" },
                    // Maximum tokens to generate
                    num_predict = 50
                };

                string json = JsonSerializer.Serialize(request);
                StringContent content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"{_ollamaUrl}/api/generate", content);
                
                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    OllamaResponse? ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseContent);
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
                // Clean up the response
                string cleanResponse = response.Trim();
                string[] validTones = new[] { 
                    "Poignant", "Melancholic", "Bittersweet", "Gut-wrenching", "Heartwarming", "Haunting", 
                    "Dark", "Bleak", "Gritty", "Cynical", "Unsettling", "Hard-boiled", "Grimdark", 
                    "Disturbing", "Horrific", "Macabre", "Grotesque", "Claustrophobic", "Intense", 
                    "Suspenseful", "Atmospheric", "Lyrical", "Surreal", "Mystical", "Dramatic", 
                    "Heroic", "Tragic", "Romantic", "Steamy", "Sweet", "Angsty", "Flirty", 
                    "Realistic", "Detached", "Upbeat", "Hopeful", "Uplifting", "Playful", 
                    "Comforting", "Cozy", "Whimsical", "Philosophical", "Psychological", "Epic"
                };
                
                List<string> foundTones = new List<string>();
                
                // First, try to parse as JSON array
                try
                {
                    string[]? jsonTones = JsonSerializer.Deserialize<string[]>(cleanResponse);
                    if (jsonTones != null && jsonTones.Length > 0)
                    {
                        foreach (string tone in jsonTones)
                        {
                            string trimmedTone = tone.Trim().Trim('"', '"'); // Remove quotes
                            if (validTones.Contains(trimmedTone))
                            {
                                foundTones.Add(trimmedTone);
                            }
                        }
                        
                        // Return up to 6 tones (as per Modelfile specification)
                        return foundTones.Take(6).ToList();
                    }
                }
                catch (JsonException)
                {
                    // Not JSON, fall back to other parsing methods
                }
                
                // Fallback: try to find tones in the response text
                string cleanResponseLower = cleanResponse.ToLower();
                foreach (string tone in validTones)
                {
                    if (cleanResponseLower.Contains(tone.ToLower()))
                    {
                        foundTones.Add(tone);
                    }
                }
                
                // If we found tones, return them (up to 6)
                if (foundTones.Count > 0)
                {
                    return foundTones.Take(6).ToList();
                }
                
                // Last resort: try to parse comma-separated values
                string[] parts = cleanResponse.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    string trimmedPart = part.Trim();
                    string? matchingTone = validTones.FirstOrDefault(t => t.ToLower() == trimmedPart);
                    if (matchingTone != null)
                    {
                        foundTones.Add(matchingTone);
                    }
                }
                
                return foundTones.Take(6).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Ollama response: {Response}", response);
                return new List<string>();
            }
        }

    }
} 