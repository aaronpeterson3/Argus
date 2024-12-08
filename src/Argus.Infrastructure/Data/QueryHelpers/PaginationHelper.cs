namespace Argus.Infrastructure.Data.QueryHelpers
{
    /// <summary>
    /// Provides extension methods for paginated database queries
    /// </summary>
    public static class PaginationHelper
    {
        /// <summary>
        /// Executes a paginated query and returns results with metadata
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="connection">Database connection</param>
        /// <param name="query">SQL query builder</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>Paginated result with items and metadata</returns>
        public static async Task<PaginatedResult<T>> PaginateAsync<T>(
            this IDbConnection connection,
            SqlBuilder query,
            int page,
            int pageSize)
        {
            var countQuery = new SqlBuilder()
                .Select("COUNT(*)")
                .From(query.FromTable)
                .Where(query.WhereClause, query.Parameters);

            var (countSql, countParams) = countQuery.Build();
            var total = await connection.ExecuteScalarAsync<int>(countSql, countParams);

            query.OrderBy(query.DefaultSort)
                 .Offset((page - 1) * pageSize)
                 .Limit(pageSize);

            var (sql, parameters) = query.Build();
            var items = await connection.QueryAsync<T>(sql, parameters);

            return new PaginatedResult<T>
            {
                Items = items.ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }
    }

    /// <summary>
    /// Contains paginated query results and metadata
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public class PaginatedResult<T>
    {
        /// <summary>
        /// Items in the current page
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }
    }
}