using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using EXE.DataAccess;
using System.Text;
using System.Security.Cryptography;
using static System.Net.WebRequestMethods;

namespace EXE.Controllers
{
    public class ChangePasswordController : Controller
    {
        private readonly Exe201Context _context;
        Random random = new Random();
        int otp;
        public ChangePasswordController(Exe201Context context)
        {
            _context = context;

        }
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserSessionID") != null)
            {
                ViewData["statusPassword"] = "changePassword";
                return View("/Views/Account/ChangePassword.cshtml");
            }
            return View("/Views/Home/Index.cshtml");
        }

        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            var userSessionID = HttpContext.Session.GetInt32("UserSessionID");
            if (userSessionID != null) //Nếu không có sesstion => chuyển sang login
            {
                var userID = _context.Users.FirstOrDefault(u => u.UserID == userSessionID);
                if (userID != null)
                {
                    var hashedPassword = HashPassword(oldPassword);
                    if (hashedPassword == userID.Password)
                    { //Nếu password cũ đúng => gửi OTP đổi password

                        HttpContext.Session.SetString("newPassword", newPassword);

                        otp = random.Next(100000, 1000000); //Random OTP
                        HttpContext.Session.SetInt32("OTPChangePassword", otp);
                        SendOTPToChangeInformation(userID.Gmail, otp.ToString());

                        return RedirectToAction("OTP");
                    }
                    else //Nếu password cũ không đúng => trở về và báo lỗi
                    {
                        TempData["statusPassword"] = "incorrectPassword";
                        return View("/Views/Account/ChangePassword.cshtml.cshtml");
                    }

                }
            }
            return View("/Views/Login/OTP.cshtml");
        }

        public IActionResult OTP()
        {
            ViewData["UserAction"] = "changePassword";
            return View("/Views/Login/OTP.cshtml");
        }

        [HttpPost]
        public IActionResult OTPChangePassword(string OTP)
        {
            var sessionOTP = HttpContext.Session.GetInt32("OTPChangePassword");
            if (OTP == sessionOTP.ToString())
            {
                var newPassword = HttpContext.Session.GetString("newPassword");
                var sessionUserID = HttpContext.Session.GetInt32("UserSessionID");
                var user = _context.Users.FirstOrDefault(u => u.UserID == sessionUserID);
                string hashedPassword = HashPassword(newPassword);
                if (user != null)
                {
                    user.Password = newPassword;
                }
                _context.Users.Update(user);
                _context.SaveChanges();
                HttpContext.Session.Remove("OTPChangePassword");
                HttpContext.Session.Remove("newPassword");

                return View("/Views/Home/Index.cshtml");
            }
            return View("Views/Home/Index.cshtml");
        }

        private void SendOTPToChangeInformation(string email, string otp)
        {
            try
            {
                var fromAddress = new MailAddress("gimmehomee@gmail.com");
                var toAddress = new MailAddress(email);
                const string frompass = "cgzobrjclhcbmggz";
                const string subject = "Gimme's OTP Verification";
                string title = "Để thay đổi mật khẩu, hãy nhập mã OTP:\n";
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
