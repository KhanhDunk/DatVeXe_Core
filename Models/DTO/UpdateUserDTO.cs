using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models.DTO
{
    public class UpdateUserDTO
    {
        [JsonIgnore]
        public int UserId { get; set; }

        [MinLength(6)]
        [MaxLength(20)]
        [RegularExpression("^[a-zA-Z0-9_]+$", ErrorMessage = "Username ch? g?m ch?, s?, _")]
        public string? Username { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public int? RoleId { get; set; }
    }
}
