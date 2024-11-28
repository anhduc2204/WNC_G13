using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Threading.Tasks;
using WNC_G13.Models;
using WNC_G13.Models.ViewModels;
using WNC_G13.Repositories;

namespace WNC_G13.Controllers
{
    public class ProjectTaskController : Controller
    {
        private readonly IProjectTaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;

        private readonly ILogger<ProjectTaskController> _logger;

        public ProjectTaskController(IProjectTaskRepository taskRepository, IProjectRepository projectRepository, ILogger<ProjectTaskController> logger)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _logger = logger;
        }


        // Hiển thị danh sách nhiệm vụ cho một dự án cụ thể
        public async Task<IActionResult> Index(int projectId)
        {
            // Lấy danh sách công việc dự án
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);

            // Lấy tên dự án từ repository
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return NotFound();  // Nếu dự án không tồn tại, trả về lỗi 404
            }

            // Truyền tên dự án vào ViewBag
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = projectId;

            // Lấy vai trò người dùng từ session
            var userRole = HttpContext.Session.GetInt32("UserRole");
            ViewBag.UserRole = userRole;

            // Kiểm tra xem tất cả các công việc trong dự án có hoàn thành hay không
            var allCompleted = tasks.All(t => t.Status == (int)Models.TaskStatus.Completed);
            var anyNotCompleted = tasks.Any(t => t.Status != (int)Models.TaskStatus.Completed);

            // Cập nhật trạng thái của dự án dựa trên trạng thái của các task
            if (allCompleted)
            {
                if (project.Status != (int)ProjectStatus.ToDo)  // Nếu tất cả task đã hoàn thành, chuyển trạng thái dự án thành "ToDo"
                {
                    project.Status = (int)ProjectStatus.ToDo;
                    await _projectRepository.UpdateAsync(project);  // Cập nhật trạng thái của dự án trong database
                }
            }
            else if (anyNotCompleted)
            {
                if (project.Status != (int)ProjectStatus.NotReady)  // Nếu có task chưa hoàn thành, chuyển trạng thái dự án thành "NotReady"
                {
                    project.Status = (int)ProjectStatus.NotReady;
                    await _projectRepository.UpdateAsync(project);  // Cập nhật trạng thái của dự án trong database
                }
            }

            // Thống kê số lượng công việc theo trạng thái
            var taskStatusCount = new
            {
                NotReady = tasks.Count(t => t.Status == (int)WNC_G13.Models.TaskStatus.NotReady),
                ToDo = tasks.Count(t => t.Status == (int)WNC_G13.Models.TaskStatus.ToDo),
                InProgress = tasks.Count(t => t.Status == (int)WNC_G13.Models.TaskStatus.InProgress),
                Completed = tasks.Count(t => t.Status == (int)WNC_G13.Models.TaskStatus.Completed),
                Delayed = tasks.Count(t => t.Status == (int)WNC_G13.Models.TaskStatus.Delayed)
            };

            // Lưu số lượng vào ViewBag để hiển thị
            ViewBag.TaskStatusCount = taskStatusCount;

            return View(tasks);
        }

        // Hiển thị form tạo nhiệm vụ mới
        [HttpGet]
        public IActionResult Create(int projectId)
        {
            // Kiểm tra vai trò người dùng và truyền vào ViewBag
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;

            if (!ViewBag.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            ViewBag.ProjectId = projectId;
            return View();
        }

        // Xử lý khi người dùng submit form tạo nhiệm vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectTask task, int projectId)
        {
            // Kiểm tra vai trò người dùng và truyền vào ViewBag
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;

            if (!ViewBag.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var projectExists = await _projectRepository.GetByIdAsync(projectId);
            if (projectExists == null)
            {
                ModelState.AddModelError("", "Dự án không tồn tại.");
                return View(task);
            }

            task.ProjectId = projectId;
            if(task.UserId != null){
                task.UserId = task.UserId >= 0 ? task.UserId : null;
            }
            await _taskRepository.AddAsync(task);
            return RedirectToAction("Index", new { projectId });
        }

        // Hiển thị form chỉnh sửa nhiệm vụ
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // Kiểm tra vai trò người dùng và truyền vào ViewBag
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;

            if (!ViewBag.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.ProjectId = task.ProjectId;
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
            return View(task);
        }

        // Xử lý khi người dùng submit form chỉnh sửa nhiệm vụ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectTask task)
        {
            // Kiểm tra vai trò người dùng và truyền vào ViewBag
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;

            if (!ViewBag.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            if (ModelState.IsValid)
            {
                ViewBag.ProjectId = task.ProjectId;
                await _taskRepository.UpdateAsync(task);
                return RedirectToAction(nameof(Index), new { projectId = task.ProjectId });
                //return RedirectToAction(nameof(AllTasks));
            }
            return View(task);
        }

        // Hiển thị trang xác nhận xóa nhiệm vụ
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // Kiểm tra vai trò người dùng và truyền vào ViewBag
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;

            if (!ViewBag.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewBag.ProjectId = task.ProjectId;
            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var task = await _taskRepository.GetByIdAsync(id);
                if (task == null)
                {
                    return Json(new { success = false, message = "Nhiệm vụ không tồn tại." });
                }

                var projectId = task.ProjectId; // Lưu lại ProjectId để quay lại trang danh sách nhiệm vụ

                await _taskRepository.DeleteAsync(id);
                return Json(new { success = true, message = "Xóa nhiệm vụ thành công.", projectId = projectId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi xóa nhiệm vụ: {ex.Message}");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa nhiệm vụ." });
            }
        }


        public async Task<IActionResult> AllTasks()
        {
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;

            if (!ViewBag.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            var tasks = await _taskRepository.GetAllTasksAsync();
            return View(tasks);
        }

        // Thêm một action để trả về trạng thái mới của dự án
        [HttpPost]
        public async Task<IActionResult> UpdateProjectStatus(int projectId)
        {
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);
            var project = await _projectRepository.GetByIdAsync(projectId);

            if (project == null)
            {
                return Json(new { success = false, message = "Dự án không tồn tại." });
            }

            // Kiểm tra nếu tất cả task đã hoàn thành hoặc có task chưa hoàn thành
            var allCompleted = tasks.All(t => t.Status == (int)Models.TaskStatus.Completed);
            var anyNotCompleted = tasks.Any(t => t.Status != (int)Models.TaskStatus.Completed);

            // Cập nhật trạng thái của dự án
            if (allCompleted)
            {
                project.Status = (int)ProjectStatus.ToDo;
            }
            else if (anyNotCompleted)
            {
                project.Status = (int)ProjectStatus.NotReady;
            }

            await _projectRepository.UpdateAsync(project);  // Cập nhật trạng thái của dự án

            // Trả về trạng thái mới của dự án
            return Json(new { success = true, status = project.Status });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTaskStatus([FromBody] TaskStatusUpdateViewModel model)
        {
            var task = await _taskRepository.GetByIdAsync(model.taskId);
            if (task == null)
            {
                return Json(new { success = false, message = "Task không tồn tại." });
            }

            task.Status = model.status;
            await _taskRepository.UpdateAsync(task);

            return Json(new { success = true });
        }

        public async Task<IActionResult> Board()
        {
            // Lấy tất cả task từ tất cả các dự án
            var allTasks = await _taskRepository.GetAllTasksAsync();

            // Thống kê số lượng công việc theo trạng thái cho tất cả các dự án
            var taskStatusCount = new
            {
                NotReady = allTasks.Count(t => t.Status == (int)Models.TaskStatus.NotReady),
                ToDo = allTasks.Count(t => t.Status == (int)Models.TaskStatus.ToDo),
                InProgress = allTasks.Count(t => t.Status == (int)Models.TaskStatus.InProgress),
                Completed = allTasks.Count(t => t.Status == (int)Models.TaskStatus.Completed),
                Delayed = allTasks.Count(t => t.Status == (int)Models.TaskStatus.Delayed)
            };

            // Truyền thống kê vào ViewBag để hiển thị trên UI
            ViewBag.TaskStatusCount = taskStatusCount;

            return View();
        }

        public IActionResult Members(int projectId)
        {
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;
            ViewBag.ProjectId = projectId; 
            // if (!ViewBag.IsAdmin)
            // {
            //     return RedirectToAction("AccessDenied", "Home");
            // }
            return View();

        }

        [HttpGet]
        public IActionResult GetMembersAndTask(int projectId)
        {
            List<ProjectMemberViewModel> projectMembers = new List<ProjectMemberViewModel>();
            var users = _taskRepository.GetUsersByProjectId(projectId);
            foreach(var user in users){
                var tasks = _taskRepository.GetTasksByProjectIdAndUserId(projectId, user.Id);
                var mem = new ProjectMemberViewModel{
                    userId = user.Id,
                    fullName = user.FullName,
                    taskTodo = tasks.Where(task => task.Status == (int)WNC_G13.Models.TaskStatus.ToDo).Count(),
                    taskInProgress = tasks.Where(task => task.Status == (int)WNC_G13.Models.TaskStatus.InProgress).Count(),
                    taskCompleted = tasks.Where(task => task.Status == (int)WNC_G13.Models.TaskStatus.Completed).Count()
                };
                projectMembers.Add(mem);
            }
            return Json(projectMembers);

        }

        [HttpGet]
        public IActionResult GetMembers(int projectId)
        {
            List<object> members = new List<object>();
            var users = _taskRepository.GetUsersByProjectId(projectId);
            foreach(var user in users){
                var mem = new {
                    userId = user.Id,
                    fullName = user.FullName,
                    email = user.Email
                };
                members.Add(mem);
            }
            return Json(members);

        }

        public IActionResult Progress(int projectId) // tiến độ
        {
            ViewBag.IsAdmin = HttpContext.Session.GetInt32("UserRole") == Role.AdminRole;
            ViewBag.ProjectId = projectId;
            return View();
        }

        [HttpGet]
        public IActionResult GetTaskProgress(int projectId)
        {
            var uId = HttpContext.Session.GetInt32("UserId");
            if(uId == null){
                return RedirectToAction("Login", "Account"); // chuyển hướng
            }
            List<MyTaskViewModel> myTasks = new List<MyTaskViewModel>();
            var tasks = _taskRepository.GetTasksByProjectIdAndUserId(projectId, uId ?? -1);
            foreach(var task in tasks){
                var pri = "";
                switch(task.Priority){
                    case (int)Priority.High:
                        pri = "Cao";
                        break;
                    case (int)Priority.Medium:
                        pri = "Trung bình";
                        break;
                    case (int)Priority.Low:
                        pri = "Thấp";
                        break;
                }
                var taskViewModel = new MyTaskViewModel{
                    Id = task.Id,
                    TaskName = task.TaskName,
                    UserId = task.UserId,
                    ProjectId = task.ProjectId,
                    Description = task.Description,
                    Status = task.Status,
                    DueDate = task.DueDate,
                    Priority = pri
                };
                myTasks.Add(taskViewModel);
            }
            return Json(myTasks);

        }

    }
}
