using System.Collections.Generic;
using System.Threading.Tasks;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public interface IDocumentRepository
    {
        Task<IEnumerable<Document>> GetDocumentsByProjectIdAsync(int projectId);
        Task<Document> GetByIdAsync(int id);
        Task<Document> AddAsync(Document document);
        Task UpdateAsync(Document document);
        Task DeleteAsync(int id);
    }
}
