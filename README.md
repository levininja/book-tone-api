# BookTone API

An AI-driven RESTful API for recommending and managing book tones using C#, Entity Framework, PostgreSQL, and Ollama with the Phi model.

## Features

- AI-powered book tone recommendations using Ollama and Phi model
- Comprehensive tone analysis with 42 detailed tone definitions
- RESTful API endpoints for managing tone recommendations
- Entity Framework Core with PostgreSQL
- Swagger/OpenAPI documentation
- Input validation and error handling
- Tone analysis and feedback system

## Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL database server
- Entity Framework Core tools
- Ollama (for AI model inference)
- Hardcover Bearer Token (for enhanced mood tag analysis)

## Setup

1. **Install Ollama:**
   ```bash
   # Download and install Ollama from https://ollama.ai
   # Then pull the Phi model:
   ollama pull phi
   ```

2. **Configure Hardcover API:**
   ```bash
   # Copy the template configuration file:
   cp appsettings.Development.template.json appsettings.Development.json
   # Edit appsettings.Development.json and add your Hardcover Bearer Token
   ```

3. **Install Entity Framework Core tools globally:**
   ```bash
   dotnet tool install --global dotnet-ef
   ```

4. **Configure PostgreSQL:**
   - Install PostgreSQL on your system
   - Create a database named `booktone_db`
   - Update the connection string in `appsettings.json` with your PostgreSQL credentials

5. **Install dependencies:**
   ```bash
   dotnet restore
   ```

6. **Run database migrations:**
   ```bash
   dotnet ef database update
   ```

7. **Start Ollama (if not already running):**
   ```bash
   ollama serve
   ```

8. **Run the application:**
   ```bash
   dotnet run
   ```

The API will be available at `http://localhost:5010` (or the port shown in the console).

**Note:** The application communicates with Ollama via HTTP API at `http://localhost:11434` and with Hardcover API for enhanced mood tag analysis. Make sure Ollama is running before using the API.

## API Endpoints

### BookToneRecommendations

- `GET /api/BookToneRecommendations` - Get AI-generated tone recommendations for a book
- `GET /api/BookToneRecommendations/{id}` - Get a specific tone recommendation
- `PUT /api/BookToneRecommendations/{id}` - Update an existing recommendation with feedback

**Note:** Recommendations are created by the AI system when you request tone analysis for a book.

### Data Model

**Request (GET):**
```json
{
  "bookTitle": "The Great Gatsby",
  "bookAuthor": "F. Scott Fitzgerald",
  "genres": ["Fiction", "Classic", "Drama"]
}
```

**Response:**
```json
{
  "id": 1,
  "bookTitle": "The Great Gatsby",
  "bookAuthor": "F. Scott Fitzgerald",
  "tones": ["Dramatic", "Intense", "Realistic"],
  "createdAt": "2024-01-01T00:00:00Z"
}
```

**Update Request (PUT):**
```json
{
  "id": 1,
  "feedback": 1,
  "tone": "Dramatic"
}
```

### Enhanced Analysis

The API now uses both:
- **Genre analysis** from the provided genres
- **Mood tag analysis** from Hardcover API (reader-generated mood tags)
- **AI analysis** using the Phi model with comprehensive tone definitions

This provides more accurate and nuanced tone recommendations based on actual reader experiences.

### Feedback System

The feedback system uses a scale from -1 to 1:
- **-1**: Negative feedback (tone recommendation was poor)
- **0**: Neutral feedback (tone recommendation was acceptable)
- **1**: Positive feedback (tone recommendation was excellent)

### Example Usage

**Get AI-generated tone recommendations for a book:**
```bash
curl -X GET "http://localhost:5010/api/BookToneRecommendations?BookTitle=The%20Great%20Gatsby&BookAuthor=F.%20Scott%20Fitzgerald&Genres=Fiction&Genres=Classic&Genres=Drama"
```

**Update a recommendation with feedback:**
```bash
curl -X PUT "http://localhost:5010/api/BookToneRecommendations/1" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "feedback": 1,
    "tone": "Dramatic"
  }'
```

**Get a specific recommendation:**
```bash
curl -X GET "http://localhost:5010/api/BookToneRecommendations/1"
```

## Swagger Documentation

When running in development mode, you can access the Swagger UI at:
`http://localhost:5010/swagger`

## AI Model Management

The application uses Ollama with the Phi model for AI-powered tone recommendations. The model is managed entirely through Ollama:

### Updating the Model
When you want to update the Phi model:
```bash
ollama pull phi
```
The application will automatically use the updated model - no additional steps required.

### Model Configuration
- **Ollama URL**: `http://localhost:11434` (default)
- **Model**: `phi` (Microsoft's Phi model)
- **API Endpoint**: `/api/generate`

The application communicates with Ollama via HTTP API calls, so no local model files are needed in the project.

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
├── Services/            # Business logic and AI integration
├── Migrations/          # Entity Framework migrations
├── Properties/          # Project properties
├── appsettings.json     # Configuration
├── Program.cs           # Application entry point
└── README.md           # This file
```

## License

This project is licensed under the MIT License. 