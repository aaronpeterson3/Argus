using Dapper;
using System.Text;

namespace Argus.Infrastructure.Data.QueryHelpers
{
    public class SqlBuilder
    {
        private StringBuilder _sql = new StringBuilder();
        private readonly List<string> _where = new();
        private readonly List<string> _orderBy = new();
        private readonly DynamicParameters _parameters = new();
        public string FromTable { get; private set; }
        public string DefaultSort { get; private set; }
        public string WhereClause => string.Join(" AND ", _where);
        public DynamicParameters Parameters => _parameters;

        public SqlBuilder Select(string columns)
        {
            _sql.Append($"SELECT {columns} ");
            return this;
        }

        public SqlBuilder From(string table)
        {
            FromTable = table;
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
            DefaultSort = column;
            _orderBy.Add($"{column} {(ascending ? "ASC" : "DESC")}");
            return this;
        }

        public SqlBuilder WithTenantId(Guid tenantId)
        {
            return Where("tenant_id =", tenantId);
        }

        public SqlBuilder Offset(int offset)
        {
            _sql.Append($"OFFSET {offset} ");
            return this;
        }

        public SqlBuilder Limit(int limit)
        {
            _sql.Append($"LIMIT {limit} ");
            return this;
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