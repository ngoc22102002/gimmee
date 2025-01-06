using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using EXE.DataAccess;

namespace EXE.Controllers
{
    public class CartController : Controller
    {
        private readonly Exe201Context _exeContext;

        public CartController(Exe201Context exeContext)
        {
            _exeContext = exeContext;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserSessionID") == null)
            {
                return View("/Views/Login/Index.cshtml");
            }
            if (HttpContext.Session.GetString("imageFrontPath") != null)
            {
                HttpContext.Session.Remove("imageFrontPath");
                HttpContext.Session.Remove("imageBackPath");
            }
            var userSessionID = HttpContext.Session.GetInt32("UserSessionID");
            var addressString = TempData["AddressString"] as string;

            // Lấy danh sách các project từ cơ sở dữ liệu và nạp dữ liệu liên kết
            var projects = _exeContext.Projects
                .Include(p => p.Material) // Bao gồm đối tượng Material
                .Include(p => p.User) // Bao gồm đối tượng User
                .Where(p => p.UserID == userSessionID)
                .ToList();


            // Truyền addressString và danh sách projects tới view
            ViewData["AddressString"] = addressString;
            var alert = TempData["alert"] as string;
            ViewData["alert"] = alert;
            return View("Index", projects);
        }

        public IActionResult InputAddress()
        {
            return View("InputAddress", "Cart");
        }

        public IActionResult DeleteProduct(int id)
        {
            if (id == 0)
            {
                ViewData["alert"] = "Nothing to delete!";
                return RedirectToAction("Index");
            }

            var checkProject = _exeContext.Projects.FirstOrDefault(p => p.ProjectID == id);
            if (checkProject != null)
            {
                var checkProjectOrder = _exeContext.ProjectOrders.FirstOrDefault(p => p.ProjectID == checkProject.ProjectID);
                if (checkProjectOrder != null)
                {
                    _exeContext.ProjectOrders.Remove(checkProjectOrder);
                    _exeContext.SaveChanges();
                }
                _exeContext.Projects.Remove(checkProject);
                _exeContext.SaveChanges();
            }
            ViewData["alert"] = "Đã xoá!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Address(string city, string district, string ward, string numberAddress)
        {
            if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(district) && !string.IsNullOrEmpty(ward) && !string.IsNullOrEmpty(numberAddress))
            {
                var address = $"{numberAddress}, {ward}, {district}, {city}";

                var userSessionID = HttpContext.Session.GetInt32("UserSessionID");

                var user = _exeContext.Users.FirstOrDefault(u => u.UserID == userSessionID);

                if (user != null)
                {
                    user.Address = address;

                    _exeContext.Users.Update(user);
                    _exeContext.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult AddProductToPayment(int id)
        {
            // Truy vấn tất cả các Project của User dựa trên UserID
            var projects = _exeContext.Projects.Where(p => p.UserID == id).ToList();

            if (projects != null && projects.Count > 0)
            {
                // Tính tổng Price của tất cả các Material liên quan đến các Project của User
                double total = 0;

                foreach (var project in projects)
                {
                    if (project.Material != null && project.Material.Price != null)
                    {
                        total += project.Material.Price.Value;
                    }
                }

                // Tạo đối tượng Order và thiết lập thuộc tính Total
                var order = new Order
                {
                    UserID = id,
                    Total = total, // Tổng giá trị Price của tất cả các Material
                    Status = "Pending", // Ví dụ, có thể thiết lập trạng thái ban đầu là Pending
                    Note = ""
                };

                // Thêm order vào context và lưu thay đổi
                _exeContext.Orders.Add(order);
                _exeContext.SaveChanges();

                // Tạo các ProjectOrder liên kết Order với các Project
                foreach (var project in projects)
                {
                    var projectOrder = new ProjectOrder
                    {
                        ProjectID = project.ProjectID,
                        OrderID = order.OrderID
                    };
                    _exeContext.ProjectOrders.Add(projectOrder);
                }
                _exeContext.SaveChanges();
            }

            // Chuyển hướng tới Index action của PaymentController với tham số id
            return RedirectToAction("Index", "Payment", new { id });
        }
    }
}
