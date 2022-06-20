using System.ComponentModel.DataAnnotations;

namespace Pustok.ViewModels
{
    public class MemberLoginViewModel
    {
        [Required]
        [MaxLength(15)]
        [MinLength(4)]
        public string UserName { get; set; }
        [Required]
        [MaxLength(15)]
        [MinLength(4)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
