using System.ComponentModel.DataAnnotations;

namespace BookToneApi.Models
{
    public class BookToneRecommendationResponseDto
    {
        public int Id { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public int Feedback { get; set; }
        public string Tone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateBookToneRecommendationDto
    {
        public int? Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string BookTitle { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(200)]
        public string BookAuthor { get; set; } = string.Empty;
        
        [Required]
        [Range(-1, 1)]
        public int Feedback { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Tone { get; set; } = string.Empty;
    }
} 