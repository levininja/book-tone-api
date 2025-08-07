using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookToneApi.Data;
using BookToneApi.Models;
using BookToneApi.Dtos;
using BookToneApi.Services;
using BookDataApi.Dtos;

namespace BookToneApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookToneRecommendationsController : ControllerBase
    {
        private readonly BookToneDbContext _context;
        private readonly IRecommenderService _recommenderService;
        private readonly IBookDataService _bookDataService;
        private readonly ILogger<BookToneRecommendationsController> _logger;

        public BookToneRecommendationsController(
            BookToneDbContext context, 
            IRecommenderService recommenderService,
            IBookDataService bookDataService,
            ILogger<BookToneRecommendationsController> logger)
        {
            _context = context;
            _recommenderService = recommenderService;
            _bookDataService = bookDataService;
            _logger = logger;
        }

        // POST: api/BookToneRecommendations
        [HttpPost]
        public async Task<IActionResult> GenerateBookToneRecommendations([FromQuery] List<int> bookIds)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int processedCount = 0;
            int failedCount = 0;
            int totalCount = bookIds.Count;

            foreach (int bookId in bookIds)
            {
                try
                {
                    // Log start
                    BatchProcessingLog startLog = new BatchProcessingLog
                    {
                        BookId = bookId,
                        Status = "Started",
                        Message = "Beginning request to generate tone recommendations",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.BatchProcessingLogs.Add(startLog);
                    await _context.SaveChangesAsync();

                    // Make call to recommendation engine
                    List<string> recommendations = await _recommenderService.GetRecommendationsAsync(bookId);

                    // Log completion
                    BatchProcessingLog completionLog = new BatchProcessingLog
                    {
                        BookId = bookId,
                        Status = "Completed",
                        Message = $"Successfully generated {recommendations.Count} recommendations",
                        CreatedAt = DateTime.UtcNow,
                        CompletedAt = DateTime.UtcNow
                    };
                    _context.BatchProcessingLogs.Add(completionLog);

                    // Save recommendations
                    List<BookToneRecommendation> bookRecommendations = recommendations.Select(tone => new BookToneRecommendation
                    {
                        BookId = bookId,
                        Feedback = 0, // Default neutral feedback
                        Tone = tone,
                        CreatedAt = DateTime.UtcNow
                    }).ToList();

                    _context.BookToneRecommendations.AddRange(bookRecommendations);
                    await _context.SaveChangesAsync();

                    processedCount++;
                    _logger.LogInformation("Successfully processed book {BookId}. Progress: {Processed}/{Total}", 
                        bookId, processedCount, totalCount);
                }
                catch (Exception ex)
                {
                    failedCount++;
                    _logger.LogError(ex, "Failed to process book {BookId}. Progress: {Processed}/{Total}, Failed: {Failed}", 
                        bookId, processedCount, totalCount, failedCount);

                    // Log error to database
                    try
                    {
                        ErrorLog errorLog = new ErrorLog
                        {
                            Source = "BatchProcessing",
                            ErrorType = ex.GetType().Name,
                            ErrorMessage = ex.Message,
                            StackTrace = ex.StackTrace,
                            BookId = bookId,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.ErrorLogs.Add(errorLog);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception logEx)
                    {
                        _logger.LogError(logEx, "Failed to log error to database for book {BookId}", bookId);
                    }
                }
            }

            _logger.LogInformation("Batch processing completed. Total: {Total}, Processed: {Processed}, Failed: {Failed}", 
                totalCount, processedCount, failedCount);

            return Ok(new { 
                TotalProcessed = processedCount, 
                TotalFailed = failedCount, 
                TotalRequested = totalCount 
            });
        }

        // GET: api/BookToneRecommendations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookToneRecommendationResponseDto>> GetBookToneRecommendation(int id)
        {
            BookToneRecommendation? recommendation = await _context.BookToneRecommendations.FindAsync(id);

            if (recommendation == null)
            {
                return NotFound();
            }

            // Get book data from book-data-api
            BookDto? bookData = await _bookDataService.GetBookByIdAsync(recommendation.BookId);
            
            BookToneRecommendationResponseDto responseDto = new BookToneRecommendationResponseDto
            {
                Id = recommendation.Id,
                BookTitle = bookData?.Title ?? $"Book {recommendation.BookId}",
                AuthorFirstName = bookData?.AuthorFirstName ?? "Unknown",
                AuthorLastName = bookData?.AuthorLastName ?? "",
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

            BookToneRecommendation? recommendation = await _context.BookToneRecommendations.FindAsync(id);

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