using DAL.BLModels;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Services
{
    public interface IVideoRepository
    {
        IEnumerable<BLVideo> GetAll();
        IEnumerable<BLVideo> GetFilteredData(string term);
        IEnumerable<BLVideo> GetPagedData(int page, int size, string orderBy, string direction);
        int GetTotalCount();
    }
}
