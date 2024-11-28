using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using WNC_G13.Models;
using WNC_G13.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using WNC_G13.Models.ViewModels;

namespace WNC_G13.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IOrganizationRepository _organizationRepository;  // Thêm Repository cho Organization

        public ProjectController(IProjectRepository projectRepository, IOrganizationRepository organizationRepository)
        {
            _projectRepository = projectRepository;
            _organizationRepository = organizationRepository;
        }

        // Hiển thị danh sách dự án của một tổ chức cụ thể
        public async Task<IActionResult> Index()
        {
            ViewBag.UserRole = HttpContext.Session.GetInt32("UserRole");
            return View();
        }

        // Hiển thị chi tiết dự án
        public async Task<IActionResult> Details(int id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // Kiểm tra quyền admin trước khi vào form tạo dự án
        [HttpGet]
        public IActionResult Create(int organizationId)
        {
            if (HttpContext.Session.GetInt32("UserRole") != Role.AdminRole)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.OrganizationId = organizationId;
            return View();
        }

        // Xử lý tạo dự án mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project, int organizationId)
        {
            if (HttpContext.Session.GetInt32("UserRole") != Role.AdminRole)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            project.CreatedBy = HttpContext.Session.GetInt32("UserId") ?? 0;
            project.CreatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                project.OrganizationId = organizationId;
                await _projectRepository.AddAsync(project);
                return RedirectToAction(nameof(Index), new { organizationId });
            }

            ViewBag.OrganizationId = organizationId;
            return View(project);
        }

        // Kiểm tra quyền admin trước khi vào form chỉnh sửa dự án[HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetInt32("UserRole") != Role.AdminRole)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            ViewBag.OrganizationId = project.OrganizationId; // Đảm bảo organizationId được gán
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View(project);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Project project)
        {
            if (HttpContext.Session.GetInt32("UserRole") != Role.AdminRole)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            // Đảm bảo OrganizationId không bị null
            if (!project.OrganizationId.HasValue)
            {
                return BadRequest("OrganizationId is required.");
            }

            if (ModelState.IsValid)
            {
                await _projectRepository.UpdateAsync(project);
                return RedirectToAction(nameof(Index), new { organizationId = project.OrganizationId });
            }

            ViewBag.OrganizationId = project.OrganizationId;
            return View(project);
        }


        // Kiểm tra quyền admin trước khi xóa dự án
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (HttpContext.Session.GetInt32("UserRole") != Role.AdminRole)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // Xử lý xác nhận xóa dự án
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetInt32("UserRole") != Role.AdminRole)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            try
            {
                await _projectRepository.DeleteAsync(id);
                return Json(new { success = true, projectId = id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa dự án không thành công: " + ex.Message });
            }
        }

        // Hiển thị tất cả các dự án
        public async Task<IActionResult> AllProjects()
        {
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;

            if (!ViewBag.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var projects = await _projectRepository.GetAllProjectsAsync();
            return View(projects);
        }

        [HttpPost]
        public async Task<IActionResult> AddMember([FromBody] AddMemberViewModel model)
        {
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;

            if (!ViewBag.IsAdmin)
            {
                return Json(null);
            }

            var result = await _projectRepository.AddMember(model.userId, model.projectId);
            return Json(result);
        }

        [HttpPost]
        public IActionResult CheckMember([FromBody] AddMemberViewModel model)
        {
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;

            if (!ViewBag.IsAdmin)
            {
                return Json(null);
            }
            var result = _projectRepository.FindMember(model.userId, model.projectId);
            if(result != null){
                return Json(new {isExist = true});
            }else{
                return Json(new {isExist = false});
            }
        }

        [HttpGet]
        public IActionResult GetAllProjects()
        {
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;
            var userId = HttpContext.Session.GetInt32("UserId");
            if (ViewBag.IsAdmin)
            {
                List<Project> projects = _projectRepository.GetProjectsByAdmin(userId ?? -1);
                List<ProjectViewModel> list = new List<ProjectViewModel>();
                foreach(var project in projects){
                    var projectViewModel = new ProjectViewModel{
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        CreatedBy = project.CreatedBy,
                        Status = project.Status
                    };
                    list.Add(projectViewModel);
                }
                return Json(list);
            }else{
                List<Project> projects = _projectRepository.GetProjectsByUserId(userId ?? -1);
                List<ProjectViewModel> list = new List<ProjectViewModel>();
                foreach(var project in projects){
                    var projectViewModel = new ProjectViewModel{
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        CreatedBy = project.CreatedBy,
                        Status = project.Status
                    };
                    list.Add(projectViewModel);
                }
                return Json(list);
            }
            
        }

    }
}
