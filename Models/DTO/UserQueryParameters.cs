using System;
using System.ComponentModel.DataAnnotations;

namespace Models.DTO
{
    public class UserQueryParameters
    {
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;
        private int _pageSize = DefaultPageSize;

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, MaxPageSize)]
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (value <= 0)
                {
                    _pageSize = DefaultPageSize;
                    return;
                }

                _pageSize = Math.Min(value, MaxPageSize);
            }
        }

        private string? _searchTerm;

        public string? SearchTerm
        {
            get => _searchTerm;
            set => _searchTerm = value;
        }

        public string? Keyword
        {
            get => _searchTerm;
            set => _searchTerm = value;
        }

        public int Skip => (Math.Max(PageNumber, 1) - 1) * PageSize;
    }
}
