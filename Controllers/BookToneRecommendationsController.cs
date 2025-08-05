using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookToneApi.Data;
using BookToneApi.Models;
using BookToneApi.Services;

namespace BookToneApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookToneRecommendationsController : ControllerBase
    {
        private readonly BookToneDbContext _context;
        private readonly IRecommenderService _recommenderService;

        public BookToneRecommendationsController(BookToneDbContext context, IRecommenderService recommenderService)
        {
            _context = context;
            _recommenderService = recommenderService;
        }

        // GET: api/BookToneRecommendations
        [HttpGet]
        public async Task<ActionResult<BookToneRecommendationResponseDto>> GetBookToneRecommendation([FromQuery] BookToneRecommendationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var recommendation = await _recommenderService.GetRecommendationAsync(
                    request.BookTitle, 
                    request.BookAuthor, 
                    request.Genres);

                // Save the recommendation to the database
                var bookToneRecommendation = new BookToneRecommendation
                {
                    BookTitle = recommendation.BookTitle,
                    BookAuthor = recommendation.BookAuthor,
                    Feedback = 0, // Default neutral feedback
                    Tone = recommendation.Tones.FirstOrDefault() ?? "Realistic", // Use first tone or default to Realistic
                    CreatedAt = recommendation.CreatedAt
                };

                _context.BookToneRecommendations.Add(bookToneRecommendation);
                await _context.SaveChangesAsync();

                // Update the response with the actual ID from the database
                recommendation.Id = bookToneRecommendation.Id;

                return Ok(recommendation);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while generating the recommendation.");
            }
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
                Tones = new List<string> { recommendation.Tone },
                CreatedAt = recommendation.CreatedAt
            };

            return Ok(responseDto);
        }

        // PUT: api/BookToneRecommendations/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBookToneRecommendation(int id, UpdateBookToneRecommendationDto updateDto)
        {
            if (id != updateDto.Id)
            {
                return BadRequest("ID mismatch");
            }

            var recommendation = await _context.BookToneRecommendations.FindAsync(id);

            if (recommendation == null)
            {
                return NotFound();
            }

            recommendation.Feedback = updateDto.Feedback;
            recommendation.Tone = updateDto.Tone;

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

        private bool BookToneRecommendationExists(int id)
        {
            return _context.BookToneRecommendations.Any(e => e.Id == id);
        }
    }
} 