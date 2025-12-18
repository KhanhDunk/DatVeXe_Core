using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO
{
    public class EmailSetting
    {
        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string FromEmail { get; set; } = null!;
        public string FromName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
