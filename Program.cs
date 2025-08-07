using Microsoft.EntityFrameworkCore;
using BookToneApi.Data;
using BookToneApi.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework with PostgreSQL
builder.Services.AddDbContext<BookToneDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));



// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add Hardcover API service
builder.Services.AddScoped<IHardcoverApiService, HardcoverApiService>();

// Add recommender service
builder.Services.AddScoped<IRecommenderService, RecommenderService>();

// Add book data service
builder.Services.AddScoped<IBookDataService, BookDataService>();

// Add resource monitoring service
builder.Services.AddScoped<IResourceMonitorService, ResourceMonitorService>();

// Add batch processing service
builder.Services.AddScoped<IBatchProcessingService, BatchProcessingService>();
builder.Services.AddHostedService<BatchProcessingService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
