using BookDataApi.Dtos;

namespace BookToneApi.Services
{
    public interface IBookDataService
    {
        Task<BookDto?> GetBookByIdAsync(int bookId);
    }
} 