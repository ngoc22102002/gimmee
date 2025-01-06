using EXE.DataAccess;
using EXE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Configuration;
using System.Text;
using System.ComponentModel;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Net;

namespace EXE.Controllers
{
    public class LoginController : Controller
    {

        private readonly Exe201Context _context;
        int otp;
        Random random = new Random();
        public LoginController(Exe201Context context)
        {
            _context = context;

        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserSessionUsername") != null)
            {
                return RedirectToAction("/Views/Home/Index.cshtml");
            }
            TempData["LoginStatus"] = "Login";
            return View("/Views/Login/Index.cshtml");
        }

        [HttpPost]
        public IActionResult Login(User user)
        {
            if (user == null)
            {
                return BadRequest("User object is null.");
            }

            // Mã hóa mật khẩu người dùng nhập vào
            string hashedPassword = HashPassword(user.Password);

            var myUser = _context.Users.FirstOrDefault(x => x.UserName == user.UserName && x.Password == hashedPassword);

            if (myUser != null)
            {
                byte[] avatarData;
                try
                {
                    avatarData = myUser.Avatar != null ? Convert.FromBase64String(myUser.Avatar) : new byte[0];
                }
                catch (FormatException ex)
                {
                    Console.WriteLine("Error converting avatar data from Base64: " + ex.Message);
                    avatarData = new byte[0];
                }

                HttpContext.Session.SetInt32("UserSessionID", myUser.UserID);
                HttpContext.Session.SetString("UserSessionUsername", myUser.UserName);
                HttpContext.Session.SetString("UserSessionPass", myUser.Password);
                HttpContext.Session.Set("UserSessionAva", avatarData);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["LoginStatus"] = "LoginFailed";
                return View("/Views/Login/Index.cshtml");
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }



        public IActionResult Home()

        {
            if (HttpContext.Session.GetString("UserSessionUsername") != null)
            {
                ViewBag.MySession = HttpContext.Session.GetString("UserSessionUsername").ToString();
                ViewBag.MySessionPass = HttpContext.Session.GetString("UserSessionPass").ToString();
                byte[] avatarData = HttpContext.Session.Get("UserSessionAva") as byte[];


                string avatarBase64 = avatarData != null ? Convert.ToBase64String(avatarData) : string.Empty;


                ViewBag.MySessionAva = avatarBase64;
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public IActionResult Logout()
        {
            if (HttpContext.Session.GetString("UserSessionUsername") != null)
            {
                HttpContext.Session.Remove("UserSessionGmail");
                HttpContext.Session.Remove("UserSessionID");
                HttpContext.Session.Remove("UserSessionUsername");
                HttpContext.Session.Remove("UserSessionPass");

                return RedirectToAction("Index", "Home");
            }
            return View();
        }


        [HttpPost]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        public IActionResult ChangePassword(string otp, string password)
        {
            var sessionOtp = HttpContext.Session.GetInt32("OTPChangePassword");
            var sessionUserID = HttpContext.Session.GetInt32("UserSessionID");

            if (sessionOtp.HasValue && sessionOtp.Value.ToString() == otp)
            {
                var user = _context.Users.FirstOrDefault(u => u.UserID == sessionUserID);
                user.Password = password;

                _context.SaveChanges();
                HttpContext.Session.Remove("OTPChangePassword");
                return RedirectToAction("Index", "Login");
            }
            else
            {
                ModelState.AddModelError("", "OTP không hợp lệ.");
                return View("Forgot");
            }
        }


    }
}
