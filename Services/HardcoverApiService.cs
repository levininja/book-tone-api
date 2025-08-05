using System.Text.Json;
using System.Net.Http;

namespace BookToneApi.Services
{
    public class HardcoverApiService : IHardcoverApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HardcoverApiService> _logger;
        private readonly string _bearerToken;
        private readonly string _graphqlEndpoint = "https://api.hardcover.app/v1/graphql";

        public HardcoverApiService(ILogger<HardcoverApiService> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _bearerToken = configuration["HardcoverBearerToken"] ?? string.Empty;
        }

        public async Task<List<string>> GetMoodTagsAsync(string bookTitle, string bookAuthor)
        {
            try
            {
                if (string.IsNullOrEmpty(_bearerToken))
                {
                    _logger.LogWarning("Hardcover Bearer Token not configured. Skipping mood tag retrieval.");
                    return new List<string>();
                }

                // Search for the book and get mood tags in one query
                var moodTags = await SearchBookAndGetMoodTagsAsync(bookTitle, bookAuthor);
                return moodTags;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mood tags for {Title} by {Author}", bookTitle, bookAuthor);
                return new List<string>();
            }
        }

        private async Task<List<string>> SearchBookAndGetMoodTagsAsync(string bookTitle, string bookAuthor)
        {
            try
            {
                var query = @"
                query SearchBooksWithMoodTags($title: String!) {
                    books(
                        limit: 5
                        where: { title: { _eq: $title } }
                        order_by: { release_year: asc }
                    ) {
                        id
                        title
                        subtitle
                        description
                        cached_contributors
                        taggings(where: { tag: { tag_category_id: { _eq: 4 } } }) {
                            tag {
                                tag
                                tag_category_id
                                count
                            }
                        }
                    }
                }";

                var variables = new
                {
                    title = bookTitle
                };

                var request = new
                {
                    query,
                    variables
                };

                var response = await MakeGraphQLRequestAsync(request);
                if (response == null) return new List<string>();

                var moodTags = new List<string>();
                var books = response.Value.GetProperty("data").GetProperty("books");

                foreach (var book in books.EnumerateArray())
                {
                    var title = book.GetProperty("title").GetString()?.ToLower();
                    var contributors = book.GetProperty("cached_contributors").GetString()?.ToLower();

                    // Check if this book matches our search criteria
                    if (title?.Contains(bookTitle.ToLower()) == true && 
                        contributors?.Contains(bookAuthor.ToLower()) == true)
                    {
                        // Extract mood tags from this book
                        var taggings = book.GetProperty("taggings");
                        foreach (var tagging in taggings.EnumerateArray())
                        {
                            var tag = tagging.GetProperty("tag");
                            var tagName = tag.GetProperty("tag").GetString();
                            var categoryId = tag.GetProperty("tag_category_id").GetInt32();

                            // Category 4 is mood tags
                            if (categoryId == 4 && !string.IsNullOrEmpty(tagName))
                            {
                                moodTags.Add(tagName);
                            }
                        }

                        // If we found a match, return the mood tags
                        if (moodTags.Count > 0)
                        {
                            _logger.LogInformation("Found mood tags for '{Title}' by {Author}: {MoodTags}", 
                                bookTitle, bookAuthor, string.Join(", ", moodTags));
                            return moodTags;
                        }
                    }
                }

                _logger.LogWarning("Book not found in Hardcover or no mood tags available: {Title} by {Author}", 
                    bookTitle, bookAuthor);
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for book and mood tags: {Title} by {Author}", bookTitle, bookAuthor);
                return new List<string>();
            }
        }

        private async Task<JsonElement?> MakeGraphQLRequestAsync(object request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_bearerToken}");

                var response = await _httpClient.PostAsync(_graphqlEndpoint, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jsonDocument = JsonDocument.Parse(responseContent);
                    return jsonDocument.RootElement;
                }
                else
                {
                    _logger.LogError("Hardcover API returned status code: {StatusCode}", response.StatusCode);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error making GraphQL request to Hardcover API");
                return null;
            }
        }
    }
} 