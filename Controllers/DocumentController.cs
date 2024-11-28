using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WNC_G13.Models;
using WNC_G13.Repositories;

namespace WNC_G13.Controllers
{
    public class DocumentController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly string _uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents");

        public DocumentController(IDocumentRepository documentRepository)
        {
            _documentRepository = documentRepository;
        }

        // Hiển thị danh sách tài liệu của một dự án cụ thể
        public async Task<IActionResult> Index(int projectId)
        {
            var documents = await _documentRepository.GetDocumentsByProjectIdAsync(projectId);
            ViewBag.ProjectId = projectId;
            ViewBag.UserRole = HttpContext.Session.GetInt32("UserRole");
            return View(documents);
        }

        // Hiển thị form tạo tài liệu mới
        [HttpGet]
        public IActionResult Create(int projectId)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View();
        }

        // Xử lý khi người dùng submit form tạo tài liệu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Document document, int projectId)
        {
            document.UploadedBy = HttpContext.Session.GetInt32("UserId") ?? 0;
            document.UploadedAt = DateTime.Now;
            document.ProjectId = projectId;

            if (ModelState.IsValid)
            {
                await _documentRepository.AddAsync(document);
                return RedirectToAction(nameof(Index), new { projectId });
            }

            ViewBag.ProjectId = projectId;
            return View(document);
        }

        // Hiển thị form chỉnh sửa tài liệu
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Document document)
        {
            if (ModelState.IsValid)
            {
                await _documentRepository.UpdateAsync(document);
                return RedirectToAction(nameof(Index), new { projectId = document.ProjectId });
            }
            return View(document);
        }

        // Hiển thị trang xác nhận xóa tài liệu
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _documentRepository.GetByIdAsync(id);
            if (document == null)
            {
                return NotFound();
            }
            return View(document);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _documentRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // up document
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, string description, int projectId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            // Nếu chưa đăng nhập, chuyển hướng về trang Login
            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Điều hướng đến trang Login trong AccountController
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("Không có tệp được chọn.");
            }

            // Tạo thư mục nếu chưa có
            if (!Directory.Exists(_uploadFolder))
            {
                Directory.CreateDirectory(_uploadFolder);
            }

            // Lưu tệp vào thư mục wwwroot/document
            var timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filePath = Path.Combine(_uploadFolder, $"{timeStamp}_{file.FileName}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var document = new Document
            {
                DocumentUrl = $"/documents/{file.FileName}",
                Description = description,
                UploadedBy = userId,
                UploadedAt = DateTime.Now,
                ProjectId = projectId
            };

            var doc = await _documentRepository.AddAsync(document);
            return Json(doc);
        }

    }
}
