using System.ComponentModel.DataAnnotations;

namespace BookToneApi.Models
{
    public class CreateBookToneRecommendationDto
    {
        [Required]
        [MaxLength(500)]
        public string BookTitle { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string BookAuthor { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 5)]
        public int Feedback { get; set; }
    }

    public class BookToneRecommendationResponseDto
    {
        public int Id { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public int Feedback { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 