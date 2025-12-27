using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System;

namespace Models.DTO
{
    public class RoleDTO
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class RoleCreateDTO
    {
        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }
    }

    public class RoleUpdateDTO
    {
        [JsonIgnore]
        public int RoleId { get; set; }

        [MaxLength(50)]
        public string? RoleName { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }
    }
}
