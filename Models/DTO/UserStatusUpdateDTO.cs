using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models.DTO
{
    public class UserStatusUpdateDTO
    {
        [JsonIgnore]
        public int UserId { get; set; }

        [Required]
        public bool? IsActive { get; set; }
    }
}
