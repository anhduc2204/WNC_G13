using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WNC_G13.Models;
using WNC_G13.Repositories;

namespace WNC_G13.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUserRepository _userRepository;

    // Thêm vào constructor để inject IUserRepository
    public HomeController(ILogger<HomeController> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        // Nếu chưa đăng nhập, chuyển hướng về trang Login
        if (userId == null)
        {
            return RedirectToAction("Login", "Account"); // Điều hướng đến trang Login trong AccountController
        }
        // Kiểm tra vai trò người dùng từ session
        var userRole = HttpContext.Session.GetInt32("UserRole");

        // Chỉ cho phép admin truy cập
        if (userRole != Role.AdminRole)
        {
            return RedirectToAction("Index", "Project");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    public async Task<IActionResult> UserProfile()
    {
        // Lấy thông tin người dùng từ session (hoặc từ cơ sở dữ liệu)
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            return RedirectToAction("Index", "Home");
        }

        // Lấy thông tin người dùng từ repository
        var user = await _userRepository.GetUserByIdAsync(userId.Value);

        if (user == null)
        {
            return RedirectToAction("Error", "Home");
        }

        return View(user);
    }

}
