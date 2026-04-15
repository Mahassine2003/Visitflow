using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.DTOs.Schema;
using VisitFlowAPI.Infrastructure.Sql;

namespace VisitFlowAPI.Controllers;

/// <summary>Introspection en lecture seule du schéma SQL Server (tables / colonnes / valeurs distinctes).</summary>
[ApiController]
[Route("api/admin/database-schema")]
[Authorize(Roles = "ADMIN,USER")]
public class DatabaseSchemaController : ControllerBase
{
    private readonly VisitFlowDbContext _db;

    public DatabaseSchemaController(VisitFlowDbContext db) => _db = db;

    [HttpGet("tables")]
    public async Task<ActionResult<IReadOnlyList<TableInfoDto>>> GetTables(CancellationToken ct)
    {
        await _db.Database.OpenConnectionAsync(ct);
        try
        {
            var connection = _db.Database.GetDbConnection();
            var list = new List<TableInfoDto>();
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT TABLE_SCHEMA, TABLE_NAME
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE'
                  AND TABLE_NAME <> '__EFMigrationsHistory'
                ORDER BY TABLE_SCHEMA, TABLE_NAME
                """;
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var schema = reader.GetString(0);
                var table = reader.GetString(1);
                if (SqlIdentifier.IsSafePart(schema) && SqlIdentifier.IsSafePart(table))
                    list.Add(new TableInfoDto(schema, table));
            }

            return Ok(list);
        }
        finally
        {
            await _db.Database.CloseConnectionAsync();
        }
    }

    [HttpGet("columns")]
    public async Task<ActionResult<IReadOnlyList<ColumnInfoDto>>> GetColumns(
        [FromQuery] string schema = "dbo",
        [FromQuery] string table = "",
        CancellationToken ct = default)
    {
        if (!SqlIdentifier.IsSafePart(schema) || !SqlIdentifier.IsSafePart(table))
            return BadRequest("Invalid schema or table name.");

        await _db.Database.OpenConnectionAsync(ct);
        try
        {
            var connection = _db.Database.GetDbConnection();
            var list = new List<ColumnInfoDto>();
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = """
                SELECT COLUMN_NAME, DATA_TYPE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table
                ORDER BY ORDINAL_POSITION
                """;
            var p0 = cmd.CreateParameter();
            p0.ParameterName = "@schema";
            p0.Value = schema;
            cmd.Parameters.Add(p0);
            var p1 = cmd.CreateParameter();
            p1.ParameterName = "@table";
            p1.Value = table;
            cmd.Parameters.Add(p1);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var col = reader.GetString(0);
                var dt = reader.GetString(1);
                if (SqlIdentifier.IsSafePart(col))
                    list.Add(new ColumnInfoDto(col, dt));
            }

            return Ok(list);
        }
        finally
        {
            await _db.Database.CloseConnectionAsync();
        }
    }

    [HttpGet("distinct-values")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetDistinctValues(
        [FromQuery] string schema = "dbo",
        [FromQuery] string table = "",
        [FromQuery] string column = "",
        CancellationToken ct = default)
    {
        if (!SqlIdentifier.IsSafePart(schema) || !SqlIdentifier.IsSafePart(table) || !SqlIdentifier.IsSafePart(column))
            return BadRequest("Invalid schema, table or column name.");

        await _db.Database.OpenConnectionAsync(ct);
        try
        {
            var connection = _db.Database.GetDbConnection();
            if (!await SqlIdentifier.ColumnExistsAsync(connection, schema, table, column, ct))
                return BadRequest("Column not found.");

            var qSchema = SqlIdentifier.Bracket(schema);
            var qTable = SqlIdentifier.Bracket(table);
            var qCol = SqlIdentifier.Bracket(column);
            var sql = $"""
                SELECT DISTINCT TOP (500) CAST({qCol} AS NVARCHAR(4000)) AS v
                FROM {qSchema}.{qTable}
                WHERE {qCol} IS NOT NULL
                ORDER BY 1
                """;

            var values = new List<string>();
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (reader.IsDBNull(0)) continue;
                var s = reader.GetString(0).Trim();
                if (s.Length > 0) values.Add(s);
            }

            return Ok(values);
        }
        finally
        {
            await _db.Database.CloseConnectionAsync();
        }
    }
}
