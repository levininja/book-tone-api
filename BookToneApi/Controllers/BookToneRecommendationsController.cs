using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookToneApi.Data;
using BookToneApi.Models;

namespace BookToneApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookToneRecommendationsController : ControllerBase
    {
        private readonly BookToneDbContext _context;

        public BookToneRecommendationsController(BookToneDbContext context)
        {
            _context = context;
        }

        // GET: api/BookToneRecommendations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookToneRecommendationResponseDto>>> GetBookToneRecommendations()
        {
            var recommendations = await _context.BookToneRecommendations
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new BookToneRecommendationResponseDto
                {
                    Id = r.Id,
                    BookTitle = r.BookTitle,
                    BookAuthor = r.BookAuthor,
                    Feedback = r.Feedback,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(recommendations);
        }

        // GET: api/BookToneRecommendations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookToneRecommendationResponseDto>> GetBookToneRecommendation(int id)
        {
            var recommendation = await _context.BookToneRecommendations.FindAsync(id);

            if (recommendation == null)
            {
                return NotFound();
            }

            var responseDto = new BookToneRecommendationResponseDto
            {
                Id = recommendation.Id,
                BookTitle = recommendation.BookTitle,
                BookAuthor = recommendation.BookAuthor,
                Feedback = recommendation.Feedback,
                CreatedAt = recommendation.CreatedAt
            };

            return Ok(responseDto);
        }

        // POST: api/BookToneRecommendations
        [HttpPost]
        public async Task<ActionResult<BookToneRecommendationResponseDto>> CreateBookToneRecommendation(CreateBookToneRecommendationDto createDto)
        {
            var recommendation = new BookToneRecommendation
            {
                BookTitle = createDto.BookTitle,
                BookAuthor = createDto.BookAuthor,
                Feedback = createDto.Feedback,
                CreatedAt = DateTime.UtcNow
            };

            _context.BookToneRecommendations.Add(recommendation);
            await _context.SaveChangesAsync();

            var responseDto = new BookToneRecommendationResponseDto
            {
                Id = recommendation.Id,
                BookTitle = recommendation.BookTitle,
                BookAuthor = recommendation.BookAuthor,
                Feedback = recommendation.Feedback,
                CreatedAt = recommendation.CreatedAt
            };

            return CreatedAtAction(nameof(GetBookToneRecommendation), new { id = recommendation.Id }, responseDto);
        }

        // PUT: api/BookToneRecommendations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBookToneRecommendation(int id, CreateBookToneRecommendationDto updateDto)
        {
            var recommendation = await _context.BookToneRecommendations.FindAsync(id);

            if (recommendation == null)
            {
                return NotFound();
            }

            recommendation.BookTitle = updateDto.BookTitle;
            recommendation.BookAuthor = updateDto.BookAuthor;
            recommendation.Feedback = updateDto.Feedback;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookToneRecommendationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/BookToneRecommendations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBookToneRecommendation(int id)
        {
            var recommendation = await _context.BookToneRecommendations.FindAsync(id);
            if (recommendation == null)
            {
                return NotFound();
            }

            _context.BookToneRecommendations.Remove(recommendation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookToneRecommendationExists(int id)
        {
            return _context.BookToneRecommendations.Any(e => e.Id == id);
        }
    }
} 