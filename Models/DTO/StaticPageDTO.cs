using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models.DTO
{
    public class StaticPageDTO
    {
        public int PageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class StaticPageCreateDTO
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Slug { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public bool? IsActive { get; set; }
    }

    public class StaticPageUpdateDTO
    {
        [JsonIgnore]
        public int PageId { get; set; }

        [MaxLength(255)]
        public string? Title { get; set; }

        [MaxLength(255)]
        public string? Slug { get; set; }

        public string? Content { get; set; }

        public bool? IsActive { get; set; }
    }
}
