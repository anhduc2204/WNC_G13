using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using WNC_G13.Models;
using WNC_G13.Repositories;

namespace WNC_G13.Controllers
{
    public class ChatController : Controller
    {
        private readonly IMessageRepository _chatRepository;
        private readonly IChannelRepository _channelRepository;

        public ChatController(IMessageRepository chatRepository, IChannelRepository channelRepository)
        {
            _chatRepository = chatRepository;
            _channelRepository = channelRepository;
        }

        [HttpGet]
        public IActionResult GetChannels()
        {
            var channels = _channelRepository.GetChannels();
            return Json(channels); // Trả về danh sách kênh dưới dạng JSON
        }

        public IActionResult Index(int channelId)
        {
            ViewBag.ChannelId = channelId;
            ViewBag.UserName = HttpContext.Session.GetString("UserName"); // Lấy tên người dùng từ session
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId"); 
            ViewBag.UserRole = HttpContext.Session.GetInt32("UserRole");
            var channels = _channelRepository.GetChannels();
            return View(channels); // Truyền danh sách kênh vào view
        }

        [HttpGet]
        public JsonResult GetMessages(int channelId)
        {
            var messages = _chatRepository.GetMessages(channelId); // Gọi phương thức đồng bộ
            return Json(messages);
        }

        [HttpPost]
        public JsonResult SendMessage(int channelId, string messageContent)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var message = new Message
            {
                ChannelId = channelId,
                UserId = userId.Value, // Lấy UserId từ session
                MessageContent = messageContent,
                CreatedAt = DateTime.Now
            };

            _chatRepository.AddMessage(message);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult CreateChannel(ChatChannel channel)
        {
            channel.CreatedAt = DateTime.Now;
            _channelRepository.CreateChannel(channel);
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult DeleteChannel(int channelId)
        {
            _channelRepository.DeleteChannel(channelId);
            return Json(new { success = true });
        }


        // Hiển thị trang quản lý tin nhắn cho admin
        public IActionResult MessageManagement()
        {
            var userRole = HttpContext.Session.GetInt32("UserRole");
            if (userRole != Role.AdminRole)
            {
                return RedirectToAction("AccessDenied", "Home");
            }
            
            var messages = _chatRepository.GetAllMessages();
            return View(messages); // Trả về view quản lý tin nhắn
        }

        // Xóa tin nhắn
        [HttpPost]
        public IActionResult DeleteMessage(int messageId)
        {
            var userRole = HttpContext.Session.GetInt32("UserRole");
            if (userRole != Role.AdminRole)
            {
                return RedirectToAction("AccessDenied", "Home");
            }

            _chatRepository.DeleteMessage(messageId);

            // Quay lại trang quản lý tin nhắn
            return RedirectToAction("MessageManagement");
        }

    }
}
