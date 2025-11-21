using ClassLibraryMySteam.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibraryMySteam.Interfaces
{
    public interface IDBService
    {
        public Task<List<WorkItem>> GetAllWorksAsync();

        public Task<List<TagItem>> GetTagsByWorkIdAsync(int workId);

        Task AddTagAsync(string work, string tag);

        public Task<bool> AddWorkAsync(WorkItem work);
    }
}
