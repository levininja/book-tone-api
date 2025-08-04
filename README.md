# BookTone API

An AI-driven RESTful API for recommending and managing book tones using C#, Entity Framework, and PostgreSQL.

## Features

- AI-powered book tone recommendations
- RESTful API endpoints for managing tone recommendations
- Entity Framework Core with PostgreSQL
- Swagger/OpenAPI documentation
- Input validation and error handling
- Tone analysis and feedback system

## Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL database server
- Entity Framework Core tools

## Setup

1. **Install Entity Framework Core tools globally:**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

2. **Configure PostgreSQL:**
   - Install PostgreSQL on your system
   - Create a database named `booktone_db`
   - Update the connection string in `appsettings.json` with your PostgreSQL credentials

3. **Install dependencies:**
   ```bash
   dotnet restore
   ```

4. **Run database migrations:**
   ```bash
   dotnet ef database update
   ```

5. **Run the application:**
   ```bash
   dotnet run
   ```

The API will be available at `http://localhost:5010` (or the port shown in the console).

## API Endpoints

### BookToneRecommendations

- `GET /api/BookToneRecommendations` - Get all tone recommendations
- `GET /api/BookToneRecommendations/{id}` - Get a specific tone recommendation
- `PUT /api/BookToneRecommendations/{id}` - Update an existing recommendation with feedback
- `DELETE /api/BookToneRecommendations/{id}` - Delete a recommendation

**Note:** Recommendations are created by the AI system, not through the API.

### Data Model

```json
{
  "id": 1,
  "bookTitle": "The Great Gatsby",
  "bookAuthor": "F. Scott Fitzgerald",
  "feedback": 1,
  "tone": "Melancholic",
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Feedback System

The feedback system uses a scale from -1 to 1:
- **-1**: Negative feedback (tone recommendation was poor)
- **0**: Neutral feedback (tone recommendation was acceptable)
- **1**: Positive feedback (tone recommendation was excellent)

### Example Usage

**Update a recommendation with feedback:**
```bash
curl -X PUT "http://localhost:5010/api/BookToneRecommendations/1" \
  -H "Content-Type: application/json" \
  -d '{
    "bookTitle": "The Great Gatsby",
    "bookAuthor": "F. Scott Fitzgerald",
    "feedback": 1,
    "tone": "Melancholic"
  }'
```

**Get all recommendations:**
```bash
curl -X GET "http://localhost:5010/api/BookToneRecommendations"
```

**Get a specific recommendation:**
```bash
curl -X GET "http://localhost:5010/api/BookToneRecommendations/1"
```

## Swagger Documentation

When running in development mode, you can access the Swagger UI at:
`http://localhost:5010/swagger`

## Development

### Adding New Migrations

When you make changes to the data models, create a new migration:

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Project Structure

```
book-tone-api/
├── Controllers/          # API controllers
├── Data/                # Entity Framework DbContext
├── Models/              # Entity models and DTOs
├── Migrations/          # Entity Framework migrations
├── Properties/          # Project properties
├── appsettings.json     # Configuration
├── Program.cs           # Application entry point
└── README.md           # This file
```

## License

This project is licensed under the MIT License. 