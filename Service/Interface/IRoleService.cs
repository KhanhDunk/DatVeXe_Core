using System.Collections.Generic;
using System.Threading.Tasks;
using Models.DTO;

namespace Service.Interface
{
    public interface IRoleService
    {
        Task<ResponseDTO<List<RoleDTO>>> GetAllAsync();
        Task<ResponseDTO<RoleDTO>> GetByIdAsync(int roleId);
        Task<ResponseDTO<RoleDTO>> CreateAsync(RoleCreateDTO dto);
        Task<ResponseDTO<RoleDTO>> UpdateAsync(int roleId, RoleUpdateDTO dto);
        Task<ResponseDTO<bool>> DeleteAsync(int roleId);
    }
}
