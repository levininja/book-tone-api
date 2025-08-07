using BookToneApi.Data;
using BookToneApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace BookToneApi.Services
{
    public class BatchProcessingService : IBatchProcessingService, IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BatchProcessingService> _logger;
        private readonly ConcurrentQueue<BatchJob> _jobQueue = new();
        private readonly ConcurrentDictionary<string, BatchProcessingStatus> _activeJobs = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1); // Only process one job at a time
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private Task? _processingTask;

        public BatchProcessingService(
            IServiceProvider serviceProvider,
            ILogger<BatchProcessingService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<string> StartBatchProcessingAsync(List<int> bookIds)
        {
            string batchId = Guid.NewGuid().ToString("N");
            
            using IServiceScope scope = _serviceProvider.CreateScope();
            BookToneDbContext context = scope.ServiceProvider.GetRequiredService<BookToneDbContext>();
            
            BatchJob batchJob = new BatchJob
            {
                BatchId = batchId,
                Status = "Queued",
                TotalBooks = bookIds.Count,
                ProcessedBooks = 0,
                FailedBooks = 0,
                CreatedAt = DateTime.UtcNow
            };
            
            context.BatchJobs.Add(batchJob);
            
            // Store the book IDs for this batch
            List<BatchJobDetail> batchJobDetails = bookIds.Select(bookId => new BatchJobDetail
            {
                BatchId = batchId,
                BookId = bookId,
                CreatedAt = DateTime.UtcNow
            }).ToList();
            
            context.BatchJobDetails.AddRange(batchJobDetails);
            await context.SaveChangesAsync();
            
            _jobQueue.Enqueue(batchJob);
            _logger.LogInformation("Queued batch job {BatchId} with {BookCount} books", batchId, bookIds.Count);
            
            return batchId;
        }

        public async Task<BatchProcessingStatus> GetBatchStatusAsync(string batchId)
        {
            if (_activeJobs.TryGetValue(batchId, out BatchProcessingStatus? status))
            {
                return status;
            }

            // Check database for completed jobs
            using IServiceScope scope = _serviceProvider.CreateScope();
            BookToneDbContext context = scope.ServiceProvider.GetRequiredService<BookToneDbContext>();
            
            BatchJob? batchJob = await context.BatchJobs
                .FirstOrDefaultAsync(j => j.BatchId == batchId);
            
            if (batchJob == null)
            {
                return new BatchProcessingStatus { Status = "NotFound" };
            }

            return new BatchProcessingStatus
            {
                BatchId = batchJob.BatchId,
                Status = batchJob.Status,
                TotalBooks = batchJob.TotalBooks,
                ProcessedBooks = batchJob.ProcessedBooks,
                FailedBooks = batchJob.FailedBooks,
                StartedAt = batchJob.StartedAt ?? batchJob.CreatedAt,
                CompletedAt = batchJob.CompletedAt,
                ErrorMessage = batchJob.ErrorMessage
            };
        }

        public async Task<List<BatchProcessingLog>> GetBatchLogsAsync(string batchId)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            BookToneDbContext context = scope.ServiceProvider.GetRequiredService<BookToneDbContext>();
            
            return await context.BatchProcessingLogs
                .Where(l => l.BatchId == batchId)
                .OrderBy(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Batch processing service starting");
            _processingTask = ProcessJobsAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Batch processing service stopping");
            _cancellationTokenSource.Cancel();
            
            if (_processingTask != null)
            {
                await _processingTask;
            }
        }

        private async Task ProcessJobsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_jobQueue.TryDequeue(out BatchJob? batchJob))
                    {
                        await ProcessBatchJobAsync(batchJob, cancellationToken);
                    }
                    else
                    {
                        await Task.Delay(1000, cancellationToken); // Wait 1 second before checking again
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in job processing loop");
                    await Task.Delay(5000, cancellationToken); // Wait 5 seconds before retrying
                }
            }
        }

        private async Task ProcessBatchJobAsync(BatchJob batchJob, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting batch job {BatchId} with {BookCount} books", 
                batchJob.BatchId, batchJob.TotalBooks);

            BatchProcessingStatus status = new BatchProcessingStatus
            {
                BatchId = batchJob.BatchId,
                Status = "Processing",
                TotalBooks = batchJob.TotalBooks,
                ProcessedBooks = 0,
                FailedBooks = 0,
                StartedAt = DateTime.UtcNow
            };

            _activeJobs[batchJob.BatchId] = status;

            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                
                using IServiceScope scope = _serviceProvider.CreateScope();
                BookToneDbContext context = scope.ServiceProvider.GetRequiredService<BookToneDbContext>();
                IRecommenderService recommenderService = scope.ServiceProvider.GetRequiredService<IRecommenderService>();

                // Update job status to processing
                batchJob.Status = "Processing";
                batchJob.StartedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();

                // Get the list of books to process (you'll need to store this or pass it differently)
                // For now, we'll process a fixed number of books as an example
                List<int> bookIds = await GetBookIdsForBatchAsync(context, batchJob.BatchId);
                
                foreach (int bookId in bookIds)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        // Log resource metrics before processing
                        using (IServiceScope resourceScope = _serviceProvider.CreateScope())
                        {
                            IResourceMonitorService resourceMonitor = resourceScope.ServiceProvider.GetRequiredService<IResourceMonitorService>();
                            await resourceMonitor.LogMetricsAsync(batchJob.BatchId, bookId);
                        }
                        
                        await ProcessSingleBookAsync(context, recommenderService, batchJob.BatchId, bookId);
                        
                        batchJob.ProcessedBooks++;
                        status.ProcessedBooks++;
                        
                        // Save progress after every book since each can take a long time
                        await context.SaveChangesAsync();
                        
                        // Log resource metrics after processing
                        using (IServiceScope resourceScope = _serviceProvider.CreateScope())
                        {
                            IResourceMonitorService resourceMonitor = resourceScope.ServiceProvider.GetRequiredService<IResourceMonitorService>();
                            await resourceMonitor.LogMetricsAsync(batchJob.BatchId, bookId);
                        }
                        
                        _logger.LogInformation("Batch {BatchId}: Processed {Processed}/{Total} books", 
                            batchJob.BatchId, batchJob.ProcessedBooks, batchJob.TotalBooks);
                    }
                    catch (Exception ex)
                    {
                        batchJob.FailedBooks++;
                        status.FailedBooks++;
                        
                        _logger.LogError(ex, "Failed to process book {BookId} in batch {BatchId}", 
                            bookId, batchJob.BatchId);
                        
                        await LogErrorAsync(context, batchJob.BatchId, bookId, ex);
                    }
                }

                // Finalize job
                batchJob.Status = "Completed";
                batchJob.CompletedAt = DateTime.UtcNow;
                status.Status = "Completed";
                status.CompletedAt = DateTime.UtcNow;
                
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Completed batch job {BatchId}: {Processed}/{Total} books processed, {Failed} failed", 
                    batchJob.BatchId, batchJob.ProcessedBooks, batchJob.TotalBooks, batchJob.FailedBooks);
            }
            catch (Exception ex)
            {
                batchJob.Status = "Failed";
                batchJob.ErrorMessage = ex.Message;
                batchJob.CompletedAt = DateTime.UtcNow;
                status.Status = "Failed";
                status.ErrorMessage = ex.Message;
                status.CompletedAt = DateTime.UtcNow;
                
                _logger.LogError(ex, "Batch job {BatchId} failed", batchJob.BatchId);
                
                using IServiceScope scope = _serviceProvider.CreateScope();
                BookToneDbContext context = scope.ServiceProvider.GetRequiredService<BookToneDbContext>();
                await context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
                _activeJobs.TryRemove(batchJob.BatchId, out _);
            }
        }

        private async Task ProcessSingleBookAsync(
            BookToneDbContext context, 
            IRecommenderService recommenderService, 
            string batchId, 
            int bookId)
        {
            // Log start
            BatchProcessingLog startLog = new BatchProcessingLog
            {
                BatchId = batchId,
                BookId = bookId,
                Status = "Started",
                Message = "Beginning request to generate tone recommendations",
                CreatedAt = DateTime.UtcNow
            };
            context.BatchProcessingLogs.Add(startLog);

            // Get recommendations
            List<string> recommendations = await recommenderService.GetRecommendationsAsync(bookId);

            // Log completion
            BatchProcessingLog completionLog = new BatchProcessingLog
            {
                BatchId = batchId,
                BookId = bookId,
                Status = "Completed",
                Message = $"Successfully generated {recommendations.Count} recommendations",
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };
            context.BatchProcessingLogs.Add(completionLog);

            // Save recommendations
            List<BookToneRecommendation> bookRecommendations = recommendations.Select(tone => new BookToneRecommendation
            {
                BookId = bookId,
                Feedback = 0,
                Tone = tone,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            context.BookToneRecommendations.AddRange(bookRecommendations);
        }

        private async Task LogErrorAsync(BookToneDbContext context, string batchId, int bookId, Exception ex)
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
            context.ErrorLogs.Add(errorLog);
        }

        private async Task<List<int>> GetBookIdsForBatchAsync(BookToneDbContext context, string batchId)
        {
            return await context.BatchJobDetails
                .Where(d => d.BatchId == batchId)
                .Select(d => d.BookId)
                .ToListAsync();
        }
    }
} 