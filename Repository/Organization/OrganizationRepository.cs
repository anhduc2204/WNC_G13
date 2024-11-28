using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly WNCG13Context _context;

        public OrganizationRepository(WNCG13Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Organization>> GetAllAsync()
        {
            return await _context.Organizations.ToListAsync();
        }

        public async Task<Organization> GetByIdAsync(int id)
        {
            return await _context.Organizations.FindAsync(id);
        }
        
        public async Task<Organization> AddAsync(Organization organization)
        {
            try
            {
                await _context.Organizations.AddAsync(organization);
                await _context.SaveChangesAsync();
                return organization;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                System.Diagnostics.Debug.WriteLine($"Error adding organization: {ex.Message}");
                throw; // Ném lại ngoại lệ nếu cần
            }
        }
        public async Task<Organization> UpdateAsync(Organization organization)
        {
            _context.Entry(organization).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return organization;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            // Tìm tổ chức bằng ID
            var organization = await _context.Organizations.FindAsync(id);
            // Nếu không tìm thấy tổ chức, trả về false
            if (organization == null) return false;

            // Xóa tổ chức khỏi DbContext
            _context.Organizations.Remove(organization);

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            // Trả về true nếu xóa thành công
            return true;
        }
        

    }
}
