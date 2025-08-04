# BookTone API

A RESTful API for managing book tone recommendations using C#, Entity Framework, and PostgreSQL.

## Features

- RESTful API endpoints for CRUD operations on book tone recommendations
- Entity Framework Core with PostgreSQL
- Swagger/OpenAPI documentation
- Input validation and error handling

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

The API will be available at `https://localhost:7001` (or the port shown in the console).

## API Endpoints

### BookToneRecommendations

- `GET /api/BookToneRecommendations` - Get all recommendations
- `GET /api/BookToneRecommendations/{id}` - Get a specific recommendation
- `POST /api/BookToneRecommendations` - Create a new recommendation
- `PUT /api/BookToneRecommendations/{id}` - Update an existing recommendation
- `DELETE /api/BookToneRecommendations/{id}` - Delete a recommendation

### Data Model

```json
{
  "id": 1,
  "bookTitle": "The Great Gatsby",
  "bookAuthor": "F. Scott Fitzgerald",
  "feedback": 4,
  "createdAt": "2024-01-01T00:00:00Z"
}
```

### Example Usage

**Create a new recommendation:**
```bash
curl -X POST "https://localhost:7001/api/BookToneRecommendations" \
  -H "Content-Type: application/json" \
  -d '{
    "bookTitle": "The Great Gatsby",
    "bookAuthor": "F. Scott Fitzgerald",
    "feedback": 4
  }'
```

**Get all recommendations:**
```bash
curl -X GET "https://localhost:7001/api/BookToneRecommendations"
```

## Swagger Documentation

When running in development mode, you can access the Swagger UI at:
`https://localhost:7001/swagger`

## Development

### Adding New Migrations

When you make changes to the data models, create a new migration:

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Project Structure

```
BookToneApi/
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