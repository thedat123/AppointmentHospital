
using Microsoft.EntityFrameworkCore;

namespace AppointmentHospital.Helpers
{
    public class Pagination<T> : List<T> 
    {
        public const int DEFAULT_PAGE_SIZE = 8; // Default page size
        public int PageSize { get; set; } // Store the page size used
        public int TotalPage { set; get; }
        public int CurrentPage { set; get; }
        public int TotalItems { set; get; }

        public Pagination(List<T> items, int currentPage, int totalItems, int pageSize = DEFAULT_PAGE_SIZE)
        {
            this.AddRange(items);
            PageSize = pageSize;
            TotalPage = (int)Math.Ceiling(totalItems / (double)pageSize);
            CurrentPage = currentPage;
            TotalItems = totalItems;
        }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPage;

        public static async Task<Pagination<T>> PaginatedList(IQueryable<T> query, int currentPage, int pageSize = DEFAULT_PAGE_SIZE)
        {
            var totalItems = query.Count();
            var paginatedList = await query.Skip((currentPage - 1) * pageSize).Take(pageSize).ToListAsync();
            return new Pagination<T>(paginatedList, currentPage, totalItems, pageSize);
        }
    }
}
