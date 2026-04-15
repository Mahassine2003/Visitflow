using System.Data.Common;

namespace VisitFlowAPI.Infrastructure.Sql;

/// <summary>Validation d’identifiants SQL Server pour introspection dynamique (noms simples).</summary>
public static class SqlIdentifier
{
    /// <summary>Lettres, chiffres, underscore ; longueur raisonnable (comme sys.columns).</summary>
    public static bool IsSafePart(string? s)
    {
        if (string.IsNullOrWhiteSpace(s) || s.Length > 128) return false;
        foreach (var c in s)
        {
            if (char.IsLetterOrDigit(c) || c == '_') continue;
            return false;
        }
        return true;
    }

    public static string Bracket(string s)
    {
        if (!IsSafePart(s)) throw new ArgumentException("Invalid SQL identifier.", nameof(s));
        return "[" + s.Replace("]", "]]", StringComparison.Ordinal) + "]";
    }

    /// <summary>La connexion doit être ouverte (utiliser <c>Database.OpenConnectionAsync</c>).</summary>
    public static async Task<bool> ColumnExistsAsync(
        DbConnection connection,
        string schema,
        string table,
        string column,
        CancellationToken ct = default)
    {
        if (!IsSafePart(schema) || !IsSafePart(table) || !IsSafePart(column)) return false;

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = @schema AND TABLE_NAME = @table AND COLUMN_NAME = @column
            """;
        var p0 = cmd.CreateParameter();
        p0.ParameterName = "@schema";
        p0.Value = schema;
        cmd.Parameters.Add(p0);
        var p1 = cmd.CreateParameter();
        p1.ParameterName = "@table";
        p1.Value = table;
        cmd.Parameters.Add(p1);
        var p2 = cmd.CreateParameter();
        p2.ParameterName = "@column";
        p2.Value = column;
        cmd.Parameters.Add(p2);
        var scalar = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt32(scalar) > 0;
    }
}
