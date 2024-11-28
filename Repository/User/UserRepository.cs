using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly WNCG13Context _context;

        public UserRepository(WNCG13Context context)
        {
            _context = context;
        }

        public async Task<User?> RegisterUserAsync(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return null; // Email đã tồn tại

            user.CreatedAt = DateTime.Now;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public List<User> FindUserByEmail(string email)
        {
            return _context.Users
                        .Where(u => u.Email.ToLower().Contains(email.ToLower()) && u.Role != Role.AdminRole ) // So sánh gần bằng, trừ tài khoản admin
                        .Select(u => new User
                            {
                                Id = u.Id,
                                FullName = u.FullName,
                                Email = u.Email
                            }
                        )
                        .ToList();
        }

        public async Task<User?> LoginUserAsync(string email, string password)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.Password = newPassword; // Thay đổi mật khẩu người dùng
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
