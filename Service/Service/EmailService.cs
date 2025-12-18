using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
    public class EmailService
    {

        public static void SendOtpEmail(string toEmail, string otp)
        {
            var fromAddress = new MailAddress("youremail@gmail.com", "YourApp");
            var toAddress = new MailAddress(toEmail);
            const string fromPassword = "yourEmailPassword";
            const string subject = "Mã OTP reset password";
            string body = $"Mã OTP của bạn là: {otp}. Hết hạn trong 5 phút.";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
