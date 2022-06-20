using System.ComponentModel.DataAnnotations;

namespace Pustok.Areas.Manage.ViewModels
{
    public class AdminRegisterViewModel
    {
        [Required]
        [MaxLength(20)]
        [MinLength(3)]
        public string FullName { get; set; }
        [Required]
        [MaxLength(20)]
        [MinLength(3)]
        public string UserName { get; set; }
        [Required]
        [MaxLength(20)]
        [MinLength(3)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [MaxLength(20)]
        [MinLength(3)]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        [Required]
        [MaxLength(25)]
        [MinLength(3)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
