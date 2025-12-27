using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.DTO;
using Models.Models;
using Service.Interface;

namespace Service.Service
{
    public class StaticPageService : IStaticPageService
    {
        private static readonly Regex WhiteSpaceRegex = new("\\s+", RegexOptions.Compiled);
        private static readonly Regex InvalidSlugRegex = new("[^a-z0-9-]", RegexOptions.Compiled);
        private static readonly Regex DashRegex = new("-{2,}", RegexOptions.Compiled);
        private readonly BookingSystemContext _context;

        private static readonly Expression<Func<StaticPage, StaticPageDTO>> PageProjection = page => new StaticPageDTO
        {
            PageId = page.PageId,
            Title = page.Title,
            Slug = page.Slug,
            Content = page.Content,
            IsActive = page.IsActive,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt,
            UpdatedBy = page.UpdatedBy
        };

        public StaticPageService(BookingSystemContext context)
        {
            _context = context;
        }

        public async Task<ResponseDTO<StaticPageDTO>> GetBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return new ResponseDTO<StaticPageDTO>(false, "Slug kh\u00F4ng h\u1EE3p l\u1EC7", null, ResponseCode.InvalidData);
            }

            try
            {
                var normalizedSlug = GenerateSlug(slug);
                if (string.IsNullOrWhiteSpace(normalizedSlug))
                {
                    return new ResponseDTO<StaticPageDTO>(false, "Slug kh\u00F4ng h\u1EE3p l\u1EC7", null, ResponseCode.InvalidData);
                }

                var page = await _context.StaticPages
                    .AsNoTracking()
                    .Where(p => p.Slug == normalizedSlug && p.IsActive == true)
                    .Select(PageProjection)
                    .FirstOrDefaultAsync();

                if (page == null)
                {
                    return new ResponseDTO<StaticPageDTO>(false, "Kh\u00F4ng t\u00ECm th\u1EA5y trang ho\u1EB7c trang \u0111\u00E3 b\u1ECB v\u00F4 hi\u1EC7u h\u00F3a", null, ResponseCode.NotFound);
                }

                return new ResponseDTO<StaticPageDTO>(true, "L\u1EA5y trang th�nh c�ng", page, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageService.GetBySlugAsync] {ex}");
                return new ResponseDTO<StaticPageDTO>(false, "Kh\u00F4ng th\u1EC3 l\u1EA5y d\u1EEF li\u1EC7u trang", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<List<StaticPageDTO>>> GetAllAsync()
        {
            try
            {
                var pages = await _context.StaticPages
                    .AsNoTracking()
                    .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                    .Select(PageProjection)
                    .ToListAsync();

                return new ResponseDTO<List<StaticPageDTO>>(true, "L\u1EA5y danh s�ch trang th�nh c�ng", pages, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageService.GetAllAsync] {ex}");
                return new ResponseDTO<List<StaticPageDTO>>(false, "Kh\u00F4ng th\u1EC3 l\u1EA5y danh s\u00E1ch trang t\u0129nh", null, ResponseCode.ServerError);
            }
        }
        
        public async Task<ResponseDTO<List<StaticPageDTO>>> GetPublishedAsync()
        {
            try
            {
                var pages = await _context.StaticPages
                    .AsNoTracking()
                    .Where(p => p.IsActive == true)
                    .OrderBy(p => p.Title)
                    .Select(PageProjection)
                    .ToListAsync();

                return new ResponseDTO<List<StaticPageDTO>>(true, "L\u1EA5y danh s�ch trang đang hiển thị thành công", pages, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageService.GetPublishedAsync] {ex}");
                return new ResponseDTO<List<StaticPageDTO>>(false, "Kh\u00F4ng th\u1EC3 l\u1EA5y danh s\u00E1ch trang đang hiển thị", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<StaticPageDTO>> CreateAsync(StaticPageCreateDTO dto, int? userId)
        {
            if (dto == null)
            {
                return new ResponseDTO<StaticPageDTO>(false, "D\u1EEF li\u1EC7u kh\u00F4ng h\u1EE3p l\u1EC7", null, ResponseCode.InvalidData);
            }

            try
            {
                var title = dto.Title?.Trim();
                if (string.IsNullOrWhiteSpace(title))
                {
                    return new ResponseDTO<StaticPageDTO>(false, "Ti\u00EAu \u0111\u1EC1 kh\u00F4ng \u0111\u01B0\u1EE3c b\u1ECF tr\u1ED1ng", null, ResponseCode.InvalidData);
                }

                if (string.IsNullOrWhiteSpace(dto.Content))
                {
                    return new ResponseDTO<StaticPageDTO>(false, "N\u1ED9i dung kh\u00F4ng \u0111\u01B0\u1EE3c b\u1ECF tr\u1ED1ng", null, ResponseCode.InvalidData);
                }

                var slugInput = string.IsNullOrWhiteSpace(dto.Slug) ? title : dto.Slug!;
                var slug = GenerateSlug(slugInput);

                if (string.IsNullOrWhiteSpace(slug))
                {
                    return new ResponseDTO<StaticPageDTO>(false, "Kh\u00F4ng th\u1EC3 t\u1EA1o slug h\u1EE3p l\u1EC7", null, ResponseCode.InvalidData);
                }

                var slugExists = await _context.StaticPages
                    .AsNoTracking()
                    .AnyAsync(p => p.Slug == slug);

                if (slugExists)
                {
                    return new ResponseDTO<StaticPageDTO>(false, "Slug \u0111\u00E3 t\u1ED3n t\u1EA1i", null, ResponseCode.InvalidData);
                }

                var now = DateTime.UtcNow;

                var page = new StaticPage
                {
                    Title = title,
                    Slug = slug,
                    Content = dto.Content,
                    IsActive = dto.IsActive ?? true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };

                _context.StaticPages.Add(page);
                await _context.SaveChangesAsync();

                var created = await GetDtoByIdAsync(page.PageId);
                return new ResponseDTO<StaticPageDTO>(true, "T\u1EA1o trang th\u00E0nh c\u00F4ng", created, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageService.CreateAsync] {ex}");
                return new ResponseDTO<StaticPageDTO>(false, "Kh\u00F4ng th\u1EC3 t\u1EA1o trang", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<StaticPageDTO>> UpdateAsync(int pageId, StaticPageUpdateDTO dto, bool isSuperAdmin, int? userId)
        {
            if (dto == null)
            {
                return new ResponseDTO<StaticPageDTO>(false, "D\u1EEF li\u1EC7u kh\u00F4ng h\u1EE3p l\u1EC7", null, ResponseCode.InvalidData);
            }

            if (!isSuperAdmin && (dto.Title != null || dto.Slug != null))
            {
                return new ResponseDTO<StaticPageDTO>(false, "B\u1EA1n kh\u00F4ng c\u00F3 quy\u1EC1n thay \u0111\u1ED5i c\u1EA5u tr\u00FAc \u0111\u01B0\u1EDDng d\u1EABn c\u1EE7a trang", null, ResponseCode.Forbidden);
            }

            try
            {
                var page = await _context.StaticPages.FirstOrDefaultAsync(p => p.PageId == pageId);
                if (page == null)
                {
                    return new ResponseDTO<StaticPageDTO>(false, "Kh\u00F4ng t\u00ECm th\u1EA5y trang", null, ResponseCode.NotFound);
                }

                if (isSuperAdmin)
                {
                    if (!string.IsNullOrWhiteSpace(dto.Title))
                    {
                        page.Title = dto.Title.Trim();
                    }

                    if (dto.Slug != null)
                    {
                        var slugCandidate = GenerateSlug(dto.Slug);
                        if (string.IsNullOrWhiteSpace(slugCandidate))
                        {
                            return new ResponseDTO<StaticPageDTO>(false, "Slug kh\u00F4ng h\u1EE3p l\u1EC7", null, ResponseCode.InvalidData);
                        }

                        var slugExists = await _context.StaticPages
                            .AsNoTracking()
                            .AnyAsync(p => p.Slug == slugCandidate && p.PageId != pageId);

                        if (slugExists)
                        {
                            return new ResponseDTO<StaticPageDTO>(false, "Slug \u0111\u00E3 t\u1ED3n t\u1EA1i", null, ResponseCode.InvalidData);
                        }

                        page.Slug = slugCandidate;
                    }
                }

                if (dto.Content != null)
                {
                    page.Content = dto.Content;
                }

                if (dto.IsActive.HasValue)
                {
                    page.IsActive = dto.IsActive.Value;
                }

                page.UpdatedAt = DateTime.UtcNow;
                page.UpdatedBy = userId;

                await _context.SaveChangesAsync();

                var updated = await GetDtoByIdAsync(page.PageId);
                return new ResponseDTO<StaticPageDTO>(true, "C\u1EADp nh\u1EADt trang th\u00E0nh c\u00F4ng", updated, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageService.UpdateAsync] {ex}");
                return new ResponseDTO<StaticPageDTO>(false, "Kh\u00F4ng th\u1EC3 c\u1EADp nh\u1EADt trang", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<bool>> DeleteAsync(int pageId)
        {
            try
            {
                var page = await _context.StaticPages.FirstOrDefaultAsync(p => p.PageId == pageId);
                if (page == null)
                {
                    return new ResponseDTO<bool>(false, "Kh\u00F4ng t\u00ECm th\u1EA5y trang", false, ResponseCode.NotFound);
                }

                _context.StaticPages.Remove(page);
                await _context.SaveChangesAsync();

                return new ResponseDTO<bool>(true, "X\u00F3a trang th\u00E0nh c\u00F4ng", true, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticPageService.DeleteAsync] {ex}");
                return new ResponseDTO<bool>(false, "Kh\u00F4ng th\u1EC3 x\u00F3a trang", false, ResponseCode.ServerError);
            }
        }

        private Task<StaticPageDTO?> GetDtoByIdAsync(int pageId)
        {
            return _context.StaticPages
                .AsNoTracking()
                .Where(p => p.PageId == pageId)
                .Select(PageProjection)
                .FirstOrDefaultAsync();
        }

        private static string GenerateSlug(string? source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return string.Empty;
            }

            var normalized = source.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();

            foreach (var ch in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(ch);
                }
            }

            var cleaned = builder.ToString().Normalize(NormalizationForm.FormC);
            cleaned = WhiteSpaceRegex.Replace(cleaned, "-");
            cleaned = InvalidSlugRegex.Replace(cleaned, string.Empty);
            cleaned = DashRegex.Replace(cleaned, "-").Trim('-');

            return cleaned;
        }
    }
}
