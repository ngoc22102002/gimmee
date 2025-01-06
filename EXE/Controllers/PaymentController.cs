using EXE.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using System.Diagnostics;

public class PaymentController : Controller
{
    private readonly PayOS payOS;
    private readonly Exe201Context _context;

    public PaymentController(Exe201Context context)
    {
        _context = context;
        payOS = new PayOS(Constants.ClientId, Constants.ApiKey, Constants.ChecksumKey);
    }

    public async Task<ActionResult> IndexAsync(int id)
    {
        if (id == 0)
        {
            // Trả về trang Cart nếu không có userID được cung cấp
            return View("/Views/Cart/Index.cshtml");
        }

        // Lấy danh sách các dự án của người dùng có userID tương ứng
        var projects = _context.Projects.Where(p => p.UserID == id).ToList();
        var user = _context.Users.FirstOrDefault(u => u.UserID == id);
        // Biến để tích lũy tổng giá trị các vật liệu từ các dự án
        decimal totalMaterialPrice = 0;
        int quantity = 0;

        foreach (var project in projects)
        {
            int materialID = (int)project.MaterialID;

            // Tìm thông tin vật liệu từ materialID
            var material = _context.Materials.FirstOrDefault(m => m.MaterialID == materialID);

            if (material != null)
            {
                decimal price = (decimal)material.Price;
                totalMaterialPrice += price; // Cộng dồn giá vật liệu vào tổng giá trị
                quantity++;
            }
        }

        // Lấy orderPayment với UserID
        // Lấy orderID mới nhất cho UserID cụ thể
        var latestOrder = _context.Orders
            .Where(o => o.UserID == id)
            .OrderByDescending(o => o.OrderID)
            .FirstOrDefault();

        if (latestOrder != null)
        {
            // Tiếp tục với các thao tác của bạn với latestOrder
            var orderPayment = latestOrder;
            orderPayment.Total = (double?)totalMaterialPrice;
            var item = new ItemData("sổ của " + user.UserName, quantity, (int)totalMaterialPrice);
            var items = new List<ItemData> { item };
            var paymentData = new PaymentData(
                (long)orderPayment.OrderID,
                (int)orderPayment.Total,
                orderPayment.Note,
                items,
                "https://gimme.id.vn/Cart", // URL thực tế sau khi deploy
                "https://gimme.id.vn" // URL thực tế sau khi deploy
            );

            CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);

            var linkCheckOut = createPayment.checkoutUrl;

            // Thay vì mở Chrome trên máy chủ, trả về phản hồi HTTP để chuyển hướng người dùng
            return Redirect(linkCheckOut);
        }
        else
        {
            // Xử lý khi không tìm thấy Order nào cho UserID này
            return View("/Views/Cart/Index.cshtml");
        }
    }
}
