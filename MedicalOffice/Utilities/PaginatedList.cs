using Microsoft.EntityFrameworkCore;

namespace MedicalOffice.Utilities
{
    // Class to handle pagination of lists
    public class PaginatedList<T> : List<T>
    {
        // Current page index
        public int PageIndex { get; private set; }

        // Total number of pages
        public int TotalPages { get; private set; }

        // Constructor to initialize the paginated list
        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        // Property to check if there is a previous page
        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 1);
            }
        }

        // Property to check if there is a next page
        public bool HasNextPage
        {
            get
            {
                return (PageIndex < TotalPages);
            }
        }

        // Static method to create a paginated list asynchronously
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            // Adjust page index if there are no items on the current page and there are items in the source
            if (items.Count() == 0 && count > 0 && pageIndex > 1)
            {
                pageIndex--;
                items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            }
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
}
