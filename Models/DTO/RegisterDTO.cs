using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO
{
    public class RegisterDTO
    {
        [Required]
        [MinLength(6)]
        [MaxLength(20)]
        [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Username chỉ gồm chữ, số, _")]
        public required string Username { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(40)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).+$", ErrorMessage = "Password phải có hoa, thường và số")]
        public required string Password { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [Phone]
        public required string Phone { get; set; }
    }

}
