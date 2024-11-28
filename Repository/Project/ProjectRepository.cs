using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly WNCG13Context _context;

        public ProjectRepository(WNCG13Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetProjectsByOrganizationIdAsync(int organizationId)
        {
            return await _context.Projects
                .Where(p => p.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public async Task AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task UpdateAsync(Project project)
        {
            var existingProject = await _context.Projects.FindAsync(project.Id);
            if (existingProject != null)
            {
                existingProject.Name = project.Name;
                existingProject.Description = project.Description;
                existingProject.OrganizationId = project.OrganizationId;
                existingProject.Status = project.Status;
                // Giữ lại giá trị CreatedBy từ dữ liệu cũ
                existingProject.CreatedBy = existingProject.CreatedBy;

                await _context.SaveChangesAsync();
            }
        }


        public async Task DeleteAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<UserProject> AddMember(int userId, int projectId)
        {
            var member = new UserProject{
                UserId = userId,
                ProjectId = projectId,
                IsPm = true
            };
            await _context.UserProjects.AddAsync(member);
            await _context.SaveChangesAsync();
            return member;
        }

        public UserProject? FindMember(int userId, int projectId)
        {
            var member = _context.UserProjects.FirstOrDefault(m => m.UserId == userId && m.ProjectId == projectId);
            
            return member;
        }

        public List<Project> GetProjectsByUserId(int userId){
            return _context.UserProjects
                .Where(p => p.UserId == userId)
                .Select(up => up.Project)
                .ToList();
        }

        public List<Project> GetProjectsByAdmin(int createdBy){
            return _context.Projects
                .Where(p => p.CreatedBy == createdBy)
                .ToList();
        }

    }
}
