using System.Collections.Generic;
using System.Threading.Tasks;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public interface IProjectTaskRepository
    {
        Task<IEnumerable<ProjectTask>> GetTasksByProjectIdAsync(int projectId);
        Task<ProjectTask> GetByIdAsync(int id);
        Task AddAsync(ProjectTask task);
        Task UpdateAsync(ProjectTask task);
        Task DeleteAsync(int id);
        Task<IEnumerable<ProjectTask>> GetAllTasksAsync();

        List<User> GetUsersByProjectId(int projectId);
        List<ProjectTask> GetTasksByProjectIdAndUserId(int projectId, int userId);
    }
}
