using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using WNC_G13.Models;
using WNC_G13.Repositories;
using WNC_G13.Models.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WNC_G13.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IOrganizationRepository _organizationRepository;

        public AccountController(IUserRepository userRepository, IOrganizationRepository organizationRepository)
        {
            _userRepository = userRepository;
            _organizationRepository = organizationRepository;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            Console.WriteLine("register: " + ModelState.IsValid);
            if (ModelState.IsValid)
            {
                var user = new User{
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    Birthday = model.Birthday,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    Role = model.Role
                };
                user = await _userRepository.RegisterUserAsync(user);
                if (user != null)
                {
                    if(user.Role == Role.AdminRole){
                        var organization = new Organization
                        {
                            Name = model.OrganizationName,
                            CreatedBy = user.Id
                        };
                        await _organizationRepository.AddAsync(organization);
                    }
                    return RedirectToAction("Login", "Account");
                }
                ModelState.AddModelError("", "Email đã tồn tại. Vui lòng thử lại.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userRepository.LoginUserAsync(email, password);
            if (user != null)
            {
                // Lưu thông tin người dùng vào session
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetInt32("UserRole", user.Role);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Manage()
        {
            // Kiểm tra xem người dùng hiện tại có phải là admin không
            var userRole = HttpContext.Session.GetInt32("UserRole");
            if (userRole != Role.AdminRole)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            // Lấy danh sách tất cả người dùng từ repository
            var users = await _userRepository.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userRole = HttpContext.Session.GetInt32("UserRole");
            // if (userRole != Role.AdminRole)
            // {
            //     return RedirectToAction("AccessDenied", "Home");
            // }

            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            var userRole = HttpContext.Session.GetInt32("UserRole");
            // if (userRole != Role.AdminRole)
            // {
            //     return RedirectToAction("AccessDenied", "Home");
            // }

            if (ModelState.IsValid)
            {
                // Nếu mật khẩu mới không được cung cấp, giữ mật khẩu cũ
                if (string.IsNullOrEmpty(user.Password))
                {
                    var existingUser = await _userRepository.GetUserByIdAsync(user.Id);
                    if (existingUser != null)
                    {
                        user.Password = existingUser.Password; // Giữ mật khẩu cũ
                    }
                }

                await _userRepository.UpdateUserAsync(user);
                HttpContext.Session.SetString("UserName", user.FullName);
                return RedirectToAction("Index", "Project", new { organizationId = 1 });
            }
            return View(user);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userRepository.DeleteUserAsync(id);
                return Json(new { success = true, message = "Xoá thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi thực hiện xoá: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _userRepository.DeleteUserAsync(id);
            return RedirectToAction("Manage");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userRepository.GetUserByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kiểm tra mật khẩu hiện tại
            if (user.Password != currentPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu hiện tại không chính xác.");
                return View();
            }

            // Kiểm tra mật khẩu mới và xác nhận mật khẩu
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu mới và xác nhận mật khẩu không khớp.");
                return View();
            }

            // Cập nhật mật khẩu mới
            var isUpdated = await _userRepository.UpdatePasswordAsync(user.Id, newPassword);
            if (isUpdated)
            {
                TempData["SuccessMessage"] = "Mật khẩu đã được thay đổi thành công!";
                return RedirectToAction("Index", "Home"); 
            }

            ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra khi thay đổi mật khẩu.");
            return View();
        }

        [HttpGet]
        public IActionResult Setting()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _userRepository.GetUserByIdAsync(userId.Value).Result;
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSetting(User model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                await _userRepository.UpdateUserAsync(model);
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Setting");
            }
            return View("Setting", model);
        }

        [HttpGet]
        public IActionResult SearchUser(string search)
        {
            List<User> results = _userRepository.FindUserByEmail(search);
            return Json(results);
        }

    }
}
