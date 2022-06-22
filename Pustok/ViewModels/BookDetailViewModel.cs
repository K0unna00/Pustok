using Pustok.Models;

namespace Pustok.ViewModels
{
    public class BookDetailViewModel
    {
        public Book Book { get; set; }
        public BookPostCommentViewModel BookComments { get; set; }
    }
}
