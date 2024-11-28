using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WNC_G13.Models;
using WNC_G13.Repositories;

namespace WNC_G13.Controllers
{
    public class OrganizationController : Controller
    {
        private readonly IOrganizationRepository _organizationRepository;

        public OrganizationController(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }

        public async Task<IActionResult> Index()
        {
            var organizations = await _organizationRepository.GetAllAsync();

            // Kiểm tra xem UserId có trong session không
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                System.Diagnostics.Debug.WriteLine("UserId is null");
            }

            ViewBag.UserId = userId;
            ViewBag.UserRole = HttpContext.Session.GetInt32("UserRole");

            return View(organizations);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userRole = HttpContext.Session.GetInt32("UserRole");
            if (userRole != Role.AdminRole)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            // Lấy UserId từ session và lưu vào ViewBag để sử dụng trong View
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Organization organization)
        {
            // Kiểm tra lại UserId từ session
            organization.CreatedBy = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (ModelState.IsValid)
            {
                await _organizationRepository.AddAsync(organization);
                return RedirectToAction(nameof(Index));
            }

            // Nếu không hợp lệ, thiết lập lại ViewBag.UserId
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View(organization);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var organization = await _organizationRepository.GetByIdAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            // Lấy UserId từ session và lưu vào ViewBag
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View(organization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Organization organization)
        {
            // Lấy UserId từ session
            organization.CreatedBy = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (id != organization.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _organizationRepository.UpdateAsync(organization);
                return RedirectToAction(nameof(Index));
            }

            // Nếu không hợp lệ, thiết lập lại ViewBag.UserId
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View(organization);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _organizationRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Xoá tổ chức thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the organization: " + ex.Message });
            }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _organizationRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
