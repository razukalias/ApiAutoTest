// SqlStep.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;


namespace TestAutomationEngine.Core
{
    public class SqlStep : ComponentBase
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public bool UseWindowsAuth { get; set; }
        public IAuthenticationStrategy? Authentication { get; set; }

        public override string ComponentType => "Sql";

        protected override async Task<ComponentResult> ExecuteCoreAsync(ExecutionContext context)
        {
            var resolvedConn = VariableResolver.Resolve(ConnectionString, context);
            var resolvedQuery = VariableResolver.Resolve(Query, context);

            if (Authentication != null)
                await Authentication.ApplyAsync(context, this);

            using var conn = new SqlConnection(resolvedConn);
            using var cmd = new SqlCommand(resolvedQuery, conn);
            foreach (var p in Parameters)
                cmd.Parameters.AddWithValue(p.Key, p.Value);

            await conn.OpenAsync(context.CancellationToken);
            var reader = await cmd.ExecuteReaderAsync(context.CancellationToken);
            var dataTable = new DataTable();
            dataTable.Load(reader);

            return new ComponentResult { Component = this, Output = dataTable };
        }
    }
}