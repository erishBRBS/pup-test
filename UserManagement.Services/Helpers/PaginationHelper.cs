using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UserManagement.Services.DTOs.CommonDTOs;

namespace UserManagement.Services.Helpers
{
    public static class PaginationHelper
    {
        private static readonly int[] AllowedPageSizes = { 10, 25, 50 };

        public static async Task<PagedResultDto<TDestination>> ToPagedResultAsync<TSource, TDestination>(
            IQueryable<TSource> query,
            int pageNumber,
            int pageSize,
            Expression<Func<TSource, TDestination>> selector,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = AllowedPageSizes.Contains(pageSize) ? pageSize : 10;

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(selector)
                .ToListAsync(cancellationToken);

            return new PagedResultDto<TDestination>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}

