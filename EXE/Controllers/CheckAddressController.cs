using EXE.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckAddressController : ControllerBase
    {
        private readonly Exe201Context _exeContext;

        public CheckAddressController(Exe201Context context)
        {
            _exeContext = context;
        }

        [HttpGet("getAddress")]
        public async Task<IActionResult> GetAddress(int userSessionID)
        {
            var user = await _exeContext.Users.FirstOrDefaultAsync(u => u.UserID == userSessionID);

            if (user == null)
            {
                return NotFound(new { address = (string)null });
            }

            return Ok(new { address = user.Address });
        }
    }
}
