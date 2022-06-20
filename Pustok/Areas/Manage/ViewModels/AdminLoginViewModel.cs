using System.ComponentModel.DataAnnotations;

namespace Pustok.Areas.Manage.ViewModels
{
    public class AdminLoginViewModel
    {
        [Required]
        [MaxLength(15)]
        [MinLength(5)]
        public string UserName { get; set; }
        [Required]
        [MaxLength(15)]
        [MinLength(4)]
        public string Password { get; set; }
    }
}
