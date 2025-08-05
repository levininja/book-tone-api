using System.ComponentModel.DataAnnotations;

namespace BookToneApi.Models
{
    public class BookToneRecommendationResponseDto
    {
        public int Id { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public List<string> Tones { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }

    public class BookToneRecommendationRequestDto
    {
        [Required]
        [MaxLength(500)]
        public string BookTitle { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string BookAuthor { get; set; } = string.Empty;
        
        [Required]
        public List<string> Genres { get; set; } = new List<string>();
    }

    public class UpdateBookToneRecommendationDto
    {
        [Required]
        public int Id { get; set; }
        
        [Required]
        [Range(-1, 1)]
        public int Feedback { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Tone { get; set; } = string.Empty;
    }
} 