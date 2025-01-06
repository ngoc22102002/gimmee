using EXE.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using NuGet.Protocol.Plugins;
using System;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using static System.Net.WebRequestMethods;

namespace EXE.Controllers
{
    public class RegisterController : Controller
    {

        private readonly Exe201Context _context;
        int otp;
        Random random = new Random();

        public RegisterController(Exe201Context context)
        {
            _context = context;

        }
        public IActionResult Index()
        {
            return View("/Views/Login/Register.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> Index(User user)
        {
            var checkUser = _context.Users.FirstOrDefault(u => u.UserName == user.UserName);
            if (checkUser == null)
            {
                user.Role = "user";

                otp = random.Next(100000, 1000000); //Random OTP
                HttpContext.Session.SetInt32("OTP", otp);
                SendOTPToEmail(user.Gmail, otp.ToString());

                TempData["UserGmail"] = user.Gmail;
                TempData["UserUsername"] = user.UserName;
                TempData["UserPassword"] = user.Password;
                TempData["UserOTP"] = otp.ToString();
                return RedirectToAction("OTP");
            }
            TempData["ValidUser"] = "This Gmail has been register!";
            return View("Register", "Login");
        }

        //Gửi OTP
        private void SendOTPToEmail(string email, string otp)
        {
            try
            {
                var fromAddress = new MailAddress("gimmehomee@gmail.com");
                var toAddress = new MailAddress(email);
                const string frompass = "cgzobrjclhcb mggz";
                const string subject = "Gimme's OTP Verification";
                string title = "Cảm ơn bạn đã quan tâm đến dịch vụ thiết kế sổ tay của Gimme, hãy nhập mã OTP này để đăng ký thành viên!\n";
                string body = otp;

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, frompass),
                    Timeout = 200000
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = title + body
                })
                {
                    smtp.Send(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public IActionResult OTP()
        {
            TempData["UserAction"] = "Register";
            return View("/Views/Login/OTP.cshtml");
        }

        [HttpPost]
        public IActionResult OTP(string username, string password, string gmail, string address, string OTP)
        {
            var sessionOTP = HttpContext.Session.GetInt32("OTP");
            if (OTP == sessionOTP.ToString())
            {
                // Mã hóa mật khẩu
                string hashedPassword = HashPassword(password);

                var newUser = new User
                {
                    UserName = username,
                    Gmail = gmail,
                    Password = hashedPassword, // Sử dụng mật khẩu đã băm
                    Address = address,
                    Role = "admin"
                };

                _context.Users.Add(newUser);
                _context.SaveChanges();
                HttpContext.Session.Remove("OTP");
                return RedirectToAction("Index");
            }
            return View("Register", "Login");
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Convert the input string to a byte array and compute the hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Convert the byte array to a string of hexadecimal characters
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

    }

}
