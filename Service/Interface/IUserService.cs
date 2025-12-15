using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interface
{
    public interface IUserService
    {
        User GetByUsername(string username);
        bool Exists(string username, string email );
        void create(User user);


        List<UserDTO> GetAll();    

    }
}
