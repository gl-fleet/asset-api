using asset_allocation_api.Context;
using asset_allocation_api.Model;

namespace asset_allocation_api.Models
{
    public class PersonnelListResult(int pageCount, IEnumerable<Personnel> personnel)
    {
        public int PageCount { get; set; } = pageCount;
        public IEnumerable<Personnel> Personnel { get; set; } = personnel;
    }
}