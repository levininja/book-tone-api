using System.ComponentModel.DataAnnotations;

namespace BookToneApi.Models
{
    public class BookToneRecommendation
    {
        public int Id { get; set; }
        
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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 