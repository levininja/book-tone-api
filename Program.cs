using Microsoft.EntityFrameworkCore;
using BookToneApi.Data;
using BookToneApi.Services;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

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
