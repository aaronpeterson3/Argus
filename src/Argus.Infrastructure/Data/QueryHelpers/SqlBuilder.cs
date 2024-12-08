namespace Argus.Infrastructure.Data.QueryHelpers
{
    public class SqlBuilder
    {
        private StringBuilder _sql = new StringBuilder();
        private readonly List<string> _where = new();
        private readonly List<string> _orderBy = new();
        private readonly DynamicParameters _parameters = new();

        public SqlBuilder Select(string columns)
        {
            _sql.Append($"SELECT {columns} ");
            return this;
        }

        public SqlBuilder From(string table)
        {
            _sql.Append($"FROM {table} ");
            return this;
        }

        public SqlBuilder Where(string condition, object value)
        {
            string paramName = $"p{_parameters.ParameterNames.Count()}"; 
            _where.Add($"{condition} @{paramName}");
            _parameters.Add(paramName, value);
            return this;
        }

        public SqlBuilder OrderBy(string column, bool ascending = true)
        {
            _orderBy.Add($"{column} {(ascending ? "ASC" : "DESC")}");
            return this;
        }

        public SqlBuilder WithTenantId(Guid tenantId)
        {
            return Where("tenant_id =", tenantId);
        }

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