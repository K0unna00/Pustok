using Pustok.Models;
using System.Collections.Generic;

namespace Pustok.ViewModels
{
    public class HomeViewModel
    {
        public List<Book> IsFeatured { get; set; }
        public List<Book> IsNew { get; set; }
        public List<Book> IsDiscounted { get; set; }
    }
}
