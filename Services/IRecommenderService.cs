using BookToneApi.Models;

namespace BookToneApi.Services
{
    public interface IRecommenderService
    {
        Task<BookToneRecommendationResponseDto> GetRecommendationAsync(string bookTitle, string bookAuthor, List<string> genres);
    }
} 