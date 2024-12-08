namespace Argus.Infrastructure.Data.QueryHelpers
{
    /// <summary>
    /// Provides fluent SQL query building capabilities with parameter binding
    /// </summary>
    public class SqlBuilder
    {
        private StringBuilder _sql = new StringBuilder();
        private readonly List<string> _where = new();
        private readonly List<string> _orderBy = new();
        private readonly DynamicParameters _parameters = new();

        /// <summary>
        /// Adds SELECT clause to the query
        /// </summary>
        /// <param name="columns">Columns to select</param>
        public SqlBuilder Select(string columns)
        {
            _sql.Append($"SELECT {columns} ");
            return this;
        }

        /// <summary>
        /// Adds FROM clause to the query
        /// </summary>
        /// <param name="table">Table name</param>
        public SqlBuilder From(string table)
        {
            _sql.Append($"FROM {table} ");
            return this;
        }

        /// <summary>
        /// Adds WHERE condition with parameterized value
        /// </summary>
        /// <param name="condition">The condition expression</param>
        /// <param name="value">The parameter value</param>
        public SqlBuilder Where(string condition, object value)
        {
            string paramName = $"p{_parameters.ParameterNames.Count()}"; 
            _where.Add($"{condition} @{paramName}");
            _parameters.Add(paramName, value);
            return this;
        }

        /// <summary>
        /// Adds ORDER BY clause
        /// </summary>
        /// <param name="column">Column to sort by</param>
        /// <param name="ascending">Sort direction</param>
        public SqlBuilder OrderBy(string column, bool ascending = true)
        {
            _orderBy.Add($"{column} {(ascending ? "ASC" : "DESC")}");
            return this;
        }

        /// <summary>
        /// Adds tenant isolation condition
        /// </summary>
        /// <param name="tenantId">The tenant ID to filter by</param>
        public SqlBuilder WithTenantId(Guid tenantId)
        {
            return Where("tenant_id =", tenantId);
        }

        /// <summary>
        /// Builds and returns the complete SQL query with parameters
        /// </summary>
        /// <returns>Tuple containing SQL string and parameters</returns>
        public (string Sql, DynamicParameters Parameters) Build()
        {
            if (_where.Any())
            {
                _sql.Append($"WHERE {string.Join(" AND ", _where)} ");
            }

            if (_orderBy.Any())
            {
                _sql.Append($"ORDER BY {string.Join(", ", _orderBy)} ");
            }

            return (_sql.ToString(), _parameters);
        }
    }
}