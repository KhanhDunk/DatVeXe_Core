using System.Collections.Generic;
using System.Threading.Tasks;
using Models.DTO;

namespace Service.Interface
{
    public interface IStaticPageService
    {
        Task<ResponseDTO<StaticPageDTO>> GetBySlugAsync(string slug);
        Task<ResponseDTO<List<StaticPageDTO>>> GetAllAsync();
        Task<ResponseDTO<List<StaticPageDTO>>> GetPublishedAsync();
        Task<ResponseDTO<StaticPageDTO>> CreateAsync(StaticPageCreateDTO dto, int? userId);
        Task<ResponseDTO<StaticPageDTO>> UpdateAsync(int pageId, StaticPageUpdateDTO dto, bool isSuperAdmin, int? userId);
        Task<ResponseDTO<bool>> DeleteAsync(int pageId);
    }
}
