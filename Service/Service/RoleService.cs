using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models.DTO;
using Models.Models;
using Service.Interface;

namespace Service.Service
{
    public class RoleService : IRoleService
    {
        private readonly BookingSystemContext _context;

        private static readonly Expression<Func<Role, RoleDTO>> RoleProjection = role => new RoleDTO
        {
            RoleId = role.RoleId,
            RoleName = role.RoleName,
            Description = role.Description,
            CreatedAt = role.CreatedAt
        };

        public RoleService(BookingSystemContext context)
        {
            _context = context;
        }

        public async Task<ResponseDTO<List<RoleDTO>>> GetAllAsync()
        {
            try
            {
                var roles = await _context.Roles
                    .AsNoTracking()
                    .OrderBy(r => r.RoleName)
                    .Select(RoleProjection)
                    .ToListAsync();

                return new ResponseDTO<List<RoleDTO>>(true, "L\u1EA5y danh s\u00E1ch role th\u00E0nh c\u00F4ng", roles, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleService.GetAllAsync] {ex}");
                return new ResponseDTO<List<RoleDTO>>(false, "Kh\u00F4ng th\u1EC3 l\u1EA5y danh s\u00E1ch role", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<RoleDTO>> GetByIdAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles
                    .AsNoTracking()
                    .Where(r => r.RoleId == roleId)
                    .Select(RoleProjection)
                    .FirstOrDefaultAsync();

                if (role == null)
                {
                    return new ResponseDTO<RoleDTO>(false, "Kh\u00F4ng t\u00ECm th\u1EA5y role", null, ResponseCode.NotFound);
                }

                return new ResponseDTO<RoleDTO>(true, "L\u1EA5y role th\u00E0nh c\u00F4ng", role, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleService.GetByIdAsync] {ex}");
                return new ResponseDTO<RoleDTO>(false, "Kh\u00F4ng th\u1EC3 l\u1EA5y role", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<RoleDTO>> CreateAsync(RoleCreateDTO dto)
        {
            if (dto == null)
            {
                return new ResponseDTO<RoleDTO>(false, "D\u1EEF li\u1EC7u kh\u00F4ng h\u1EE3p l\u1EC7", null, ResponseCode.InvalidData);
            }

            try
            {
                var roleName = dto.RoleName?.Trim();
                if (string.IsNullOrWhiteSpace(roleName))
                {
                    return new ResponseDTO<RoleDTO>(false, "RoleName kh\u00F4ng \u0111\u01B0\u1EE3c b\u1ECF tr\u1ED1ng", null, ResponseCode.InvalidData);
                }

                var exists = await _context.Roles
                    .AsNoTracking()
                    .AnyAsync(r => r.RoleName == roleName);

                if (exists)
                {
                    return new ResponseDTO<RoleDTO>(false, "RoleName \u0111\u00E3 t\u1ED3n t\u1EA1i", null, ResponseCode.InvalidData);
                }

                var role = new Role
                {
                    RoleName = roleName,
                    Description = dto.Description?.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                var created = await GetRoleDtoById(role.RoleId);
                return new ResponseDTO<RoleDTO>(true, "T\u1EA1o role th\u00E0nh c\u00F4ng", created, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleService.CreateAsync] {ex}");
                return new ResponseDTO<RoleDTO>(false, "Kh\u00F4ng th\u1EC3 t\u1EA1o role", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<RoleDTO>> UpdateAsync(int roleId, RoleUpdateDTO dto)
        {
            if (dto == null)
            {
                return new ResponseDTO<RoleDTO>(false, "D\u1EEF li\u1EC7u kh\u00F4ng h\u1EE3p l\u1EC7", null, ResponseCode.InvalidData);
            }

            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
                if (role == null)
                {
                    return new ResponseDTO<RoleDTO>(false, "Kh\u00F4ng t\u00ECm th\u1EA5y role", null, ResponseCode.NotFound);
                }

                if (!string.IsNullOrWhiteSpace(dto.RoleName))
                {
                    var roleName = dto.RoleName.Trim();
                    var exists = await _context.Roles
                        .AsNoTracking()
                        .AnyAsync(r => r.RoleName == roleName && r.RoleId != roleId);

                    if (exists)
                    {
                        return new ResponseDTO<RoleDTO>(false, "RoleName \u0111\u00E3 t\u1ED3n t\u1EA1i", null, ResponseCode.InvalidData);
                    }

                    role.RoleName = roleName;
                }

                if (dto.Description != null)
                {
                    role.Description = dto.Description.Trim();
                }

                await _context.SaveChangesAsync();

                var updated = await GetRoleDtoById(role.RoleId);
                return new ResponseDTO<RoleDTO>(true, "C\u1EADp nh\u1EADt role th\u00E0nh c\u00F4ng", updated, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleService.UpdateAsync] {ex}");
                return new ResponseDTO<RoleDTO>(false, "Kh\u00F4ng th\u1EC3 c\u1EADp nh\u1EADt role", null, ResponseCode.ServerError);
            }
        }

        public async Task<ResponseDTO<bool>> DeleteAsync(int roleId)
        {
            try
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
                if (role == null)
                {
                    return new ResponseDTO<bool>(false, "Kh\u00F4ng t\u00ECm th\u1EA5y role", false, ResponseCode.NotFound);
                }

                var isRoleInUse = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.RoleId == roleId);

                if (isRoleInUse)
                {
                    return new ResponseDTO<bool>(false, "Kh\u00F4ng th\u1EC3 x\u00F3a role \u0111ang \u0111\u01B0\u1EE3c s\u1EED d\u1EE5ng", false, ResponseCode.InvalidData);
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return new ResponseDTO<bool>(true, "X\u00F3a role th\u00E0nh c\u00F4ng", true, ResponseCode.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleService.DeleteAsync] {ex}");
                return new ResponseDTO<bool>(false, "Kh\u00F4ng th\u1EC3 x\u00F3a role", false, ResponseCode.ServerError);
            }
        }

        private Task<RoleDTO?> GetRoleDtoById(int roleId)
        {
            return _context.Roles
                .AsNoTracking()
                .Where(r => r.RoleId == roleId)
                .Select(RoleProjection)
                .FirstOrDefaultAsync();
        }
    }
}
