namespace BookToneApi.Dtos
{
    public class BookToneRecommendationsResponseDto
    {
        public List<List<string>> Recommendations { get; set; } = new List<List<string>>();
    }
} 