using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models.DTO
{
    public class AdminCreateUserDTO : RegisterDTO
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "RoleId không hợp lệ")]
        [JsonPropertyName("roleId")]
        public int RoleId { get; set; }
    }
}
