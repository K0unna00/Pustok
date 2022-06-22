using System.ComponentModel.DataAnnotations;

namespace Pustok.ViewModels
{
    public class BookPostCommentViewModel
    {
        public int BookId { get; set; }
        [Required]
        [Range(1, 5)]
        public int Rate { get; set; }
        [Required]
        [MaxLength(250)]
        public string Text { get; set; }
    }
}
