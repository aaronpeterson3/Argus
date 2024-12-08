namespace Argus.Infrastructure.Data.QueryHelpers
{
    public static class PaginationHelper
    {
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

    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}