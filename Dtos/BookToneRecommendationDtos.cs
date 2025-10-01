using System.ComponentModel.DataAnnotations;

namespace BookToneApi.Dtos
{
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

    public class BookToneRecommendationItemDto
    {
        public int RecommendationId { get; set; }
        public int BookId { get; set; }
        public string Tone { get; set; } = string.Empty;
    }


} 