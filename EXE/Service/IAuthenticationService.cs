namespace EXE.Service
{
    public interface IAuthenticationService
    {
        bool AuthenticateUser(string username, string password);
    }

    public class AuthenticationService : IAuthenticationService
    {
        public bool AuthenticateUser(string username, string password)
        {
            // Thực hiện logic xác thực người dùng ở đây
            // Ví dụ: kiểm tra thông tin đăng nhập trong cơ sở dữ liệu

            // Code ví dụ (chỉ để minh họa, không nên sử dụng trong môi trường thực tế)
            if (username == "admin" && password == "password")
            {
                return true; // Xác thực thành công
            }
            else
            {
                return false; // Xác thực thất bại
            }
        }
    }

}
