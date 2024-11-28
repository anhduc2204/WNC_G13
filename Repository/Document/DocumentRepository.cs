using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WNC_G13.Models;

namespace WNC_G13.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly WNCG13Context _context;

        public DocumentRepository(WNCG13Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetDocumentsByProjectIdAsync(int projectId)
        {
            return await _context.Documents.Where(d => d.ProjectId == projectId).ToListAsync();
        }

        public async Task<Document> GetByIdAsync(int id)
        {
            return await _context.Documents.FindAsync(id);
        }

        public async Task<Document> AddAsync(Document document)
        {
            _context.Documents.Add(document);
            await _context.SaveChangesAsync();
            return document;
        }

        public async Task UpdateAsync(Document document)
        {
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var document = await GetByIdAsync(id);
            if (document != null)
            {
                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
            }
        }
    }
}
