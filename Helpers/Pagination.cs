
using Microsoft.EntityFrameworkCore;

namespace AppointmentHospital.Helpers
{
    public class Pagination<T> : List<T> 
    {
        public const int PAGE_SIZE = 10;
        public int TotalPage { set; get; }
        public int CurrentPage { set; get; }
        public int TotalItems {set;get;}
        
        public Pagination(List<T> item, int currentPage, int totalItems)
        {
            this.AddRange(item);
            TotalPage = (int)Math.Ceiling(totalItems /(double) PAGE_SIZE);
            CurrentPage = currentPage;
            TotalItems = totalItems;
        }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPage;
        public static async Task<Pagination<T>> PaginatedList(IQueryable<T> query, int currentPage)
        {
            var totalItems = query.Count();
            var paginatedList = await query.Skip((currentPage - 1) * PAGE_SIZE).Take(PAGE_SIZE).ToListAsync();
            return new Pagination<T>(paginatedList, currentPage, totalItems);
        }
    }
}
