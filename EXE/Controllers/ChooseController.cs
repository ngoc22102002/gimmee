using EXE.DataAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EXE.Controllers
{
    public class ChooseController : Controller
    {
        private readonly Exe201Context _exeContext;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ChooseController(Exe201Context exeContext, IWebHostEnvironment hostingEnvironment)
        {
            _exeContext = exeContext;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserSessionID") != null)
            {
                if (HttpContext.Session.GetString("ChooseError") != null)
                {
                    ViewData["ChooseError"] = "Error";
                    HttpContext.Session.Remove("ChooseError");
                    return View("/Views/Choose/Index.cshtml");
                }
                else
                {
                    ViewData["ChooseError"] = null; // Đảm bảo là ViewData["ChooseError"] được đặt trong mọi trường hợp
                }
                return View();
            }
            return RedirectToAction("Index", "Login");
        }


        [HttpPost]
        public IActionResult Choose([FromForm] string PaperType, [FromForm] string Size, [FromForm] string BookCuffColor, [FromForm] int NumberOfPages, [FromForm] string NotebookName, [FromForm] string NotebookDescription, [FromForm] String action)
        {
            var userSessionID = HttpContext.Session.GetInt32("UserSessionID");
            if (userSessionID != null)
            {
                if (!string.IsNullOrEmpty(PaperType) && !string.IsNullOrEmpty(Size) && !string.IsNullOrEmpty(BookCuffColor) && NumberOfPages != null)
                {
                    if ("design".Equals(action))
                    {
                        HttpContext.Session.SetString("PaperType", PaperType);
                        HttpContext.Session.SetString("Size", Size);
                        HttpContext.Session.SetString("BookCuffColor", BookCuffColor);
                        HttpContext.Session.SetInt32("NumberOfPages", NumberOfPages);
                        if (!String.IsNullOrEmpty(NotebookName))
                        {
                            HttpContext.Session.SetString("NotebookName", NotebookName);
                        }
                        if (!String.IsNullOrEmpty(NotebookDescription))
                        {
                            HttpContext.Session.SetString("NotebookDescription", NotebookDescription);
                        }

                        return RedirectToAction("Index", "Design");
                    }
                    else
                    {
                        // Lấy thông tin material từ cơ sở dữ liệu
                        var material = _exeContext.Materials.FirstOrDefault(m => m.NumberOfPage == NumberOfPages && m.Size == Size && m.PaperName == PaperType);
                        if (material == null)
                        {
                            return BadRequest("Material not found.");
                        }

                        var springColor = BookCuffColor;
                        var imageFrontTempPath = HttpContext.Session.GetString("imageFrontPathTemp");
                        var imageBackTempPath = HttpContext.Session.GetString("imageBackPathTemp");

                        // Kiểm tra xem ảnh tạm thời đã được upload chưa
                        if (string.IsNullOrEmpty(imageFrontTempPath) || string.IsNullOrEmpty(imageBackTempPath))
                        {
                            return BadRequest("Images not uploaded.");
                        }

                        // Tạo đối tượng project và lưu vào cơ sở dữ liệu
                        var project = new Project
                        {
                            UserID = userSessionID,
                            MaterialID = material.MaterialID,
                            ImageFront = "",
                            ImageBack = "",
                            Note = NotebookDescription, // Gán mô tả của notebook từ form
                            BookName = NotebookName
                        };

                        _exeContext.Projects.Add(project);
                        _exeContext.SaveChanges();

                        // Cập nhật đường dẫn ảnh với ProjectID
                        string userFolder = Path.Combine(_hostingEnvironment.WebRootPath, "image", userSessionID.ToString(), project.ProjectID.ToString());
                        if (!Directory.Exists(userFolder))
                        {
                            Directory.CreateDirectory(userFolder);
                        }

                        string fileNameFront = "textures1.png";
                        string filePathFront = Path.Combine(userFolder, fileNameFront);
                        if (System.IO.File.Exists(filePathFront))
                        {
                            System.IO.File.Delete(filePathFront);
                        }
                        System.IO.File.Move(imageFrontTempPath, filePathFront);

                        string fileNameBack = "textures2.png";
                        string filePathBack = Path.Combine(userFolder, fileNameBack);
                        if (System.IO.File.Exists(filePathBack))
                        {
                            System.IO.File.Delete(filePathBack);
                        }
                        System.IO.File.Move(imageBackTempPath, filePathBack);

                        // Cập nhật đường dẫn ảnh vào session và cơ sở dữ liệu
                        string imagePathFront = $"/image/{userSessionID}/{project.ProjectID}/{fileNameFront}";
                        string imagePathBack = $"/image/{userSessionID}/{project.ProjectID}/{fileNameBack}";
                        HttpContext.Session.SetString("imageFrontPath", imagePathFront);
                        HttpContext.Session.SetString("imageBackPath", imagePathBack);

                        project.ImageFront = imagePathFront;
                        project.ImageBack = imagePathBack;

                        // Gán tên của notebook từ form
                        project.Note = NotebookName;

                        _exeContext.SaveChanges();

                        // Sau khi thêm vào giỏ hàng, điều hướng người dùng đến trang Cart
                        return RedirectToAction("Index", "Choose");

                    }

                }
                else
                {
                    HttpContext.Session.SetString("ChooseError", "Error");
                    return RedirectToAction("Index", "Choose");
                }

            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        [HttpPost]
        public async Task<IActionResult> UploadFrontImages(IFormFile image, [FromForm] string imageType)
        {
            var userSessionID = HttpContext.Session.GetInt32("UserSessionID");
            if (userSessionID == null)
            {
                return BadRequest("User session ID is not available.");
            }

            if (image != null && image.Length > 0)
            {
                try
                {
                    // Tạo thư mục tạm thời nếu chưa tồn tại
                    string tempFolder = Path.Combine(_hostingEnvironment.WebRootPath, "temp");
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }

                    // Tạo tên tệp ảnh front tạm thời
                    string fileNameFront = $"{Guid.NewGuid()}_textures1.png";
                    string filePathFront = Path.Combine(tempFolder, fileNameFront);

                    // Lưu ảnh front vào thư mục tạm thời
                    using (var stream = new FileStream(filePathFront, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn ảnh front tạm thời vào session
                    HttpContext.Session.SetString("imageFrontPathTemp", filePathFront);
                    return Ok(new { tempPath = filePathFront });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest("No image uploaded.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadBackImages(IFormFile image, [FromForm] string imageType)
        {
            var userSessionID = HttpContext.Session.GetInt32("UserSessionID");
            if (userSessionID == null)
            {
                return BadRequest("User session ID is not available.");
            }

            if (image != null && image.Length > 0)
            {
                try
                {
                    // Tạo thư mục tạm thời nếu chưa tồn tại
                    string tempFolder = Path.Combine(_hostingEnvironment.WebRootPath, "temp");
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }

                    // Tạo tên tệp ảnh back tạm thời
                    string fileNameBack = $"{Guid.NewGuid()}_textures2.png";
                    string filePathBack = Path.Combine(tempFolder, fileNameBack);

                    // Lưu ảnh back vào thư mục tạm thời
                    using (var stream = new FileStream(filePathBack, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    // Lưu đường dẫn ảnh back tạm thời vào session
                    HttpContext.Session.SetString("imageBackPathTemp", filePathBack);
                    return Ok(new { tempPath = filePathBack });
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest("No image uploaded.");
            }
        }
    }
}
