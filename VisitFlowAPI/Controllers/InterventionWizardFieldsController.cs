using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitFlowAPI.Data;
using VisitFlowAPI.DTOs.Interventions;
using VisitFlowAPI.Infrastructure.Sql;
using VisitFlowAPI.Models;

namespace VisitFlowAPI.Controllers;

[ApiController]
[Route("api/admin/intervention-wizard-fields")]
[Authorize(Roles = "ADMIN,USER")]
public class InterventionWizardFieldsController : ControllerBase
{
    private readonly VisitFlowDbContext _db;

    public InterventionWizardFieldsController(VisitFlowDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InterventionWizardFieldDefinitionDto>>> GetAll()
    {
        var entities = await _db.InterventionWizardFieldDefinitions
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var rows = entities.Select(MapToDto).ToList();
        return Ok(rows);
    }

    [HttpPost]
    public async Task<ActionResult<InterventionWizardFieldDefinitionDto>> Create(
        [FromBody] InterventionWizardFieldCreateDto dto,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Label))
            return BadRequest("Label is required.");

        var ft = Enum.IsDefined(typeof(InterventionWizardFieldType), dto.FieldType)
            ? (InterventionWizardFieldType)dto.FieldType
            : InterventionWizardFieldType.Text;

        var mode = dto.CreationMode == (int)InterventionWizardFieldCreationMode.DatabaseBinding
            ? InterventionWizardFieldCreationMode.DatabaseBinding
            : InterventionWizardFieldCreationMode.CustomField;

        string? srcSchema = null;
        string? srcTable = null;
        string? srcColumn = null;

        if (mode == InterventionWizardFieldCreationMode.DatabaseBinding)
        {
            srcSchema = string.IsNullOrWhiteSpace(dto.SourceSchema) ? "dbo" : dto.SourceSchema!.Trim();
            srcTable = dto.SourceTable?.Trim() ?? "";
            srcColumn = dto.SourceColumn?.Trim() ?? "";
            if (!SqlIdentifier.IsSafePart(srcSchema) || !SqlIdentifier.IsSafePart(srcTable) || !SqlIdentifier.IsSafePart(srcColumn))
                return BadRequest("Invalid schema, table or column name.");
            if (ft != InterventionWizardFieldType.Select)
                return BadRequest("A field linked to the database must use type « Selection (dropdown) ».");

            await _db.Database.OpenConnectionAsync(ct);
            try
            {
                var connection = _db.Database.GetDbConnection();
                if (!await SqlIdentifier.ColumnExistsAsync(connection, srcSchema, srcTable, srcColumn, ct))
                    return BadRequest("Table or column not found in the database.");
            }
            finally
            {
                await _db.Database.CloseConnectionAsync();
            }
        }
        else if (ft == InterventionWizardFieldType.Select)
        {
            var opt = SerializeOptions(dto.FieldOptions);
            if (string.IsNullOrWhiteSpace(opt))
                return BadRequest("Add at least one choice for a selection field.");
        }

        var baseKey = Slugify(dto.Label);
        var key = await EnsureUniqueKeyAsync(baseKey);

        var maxOrder = await _db.InterventionWizardFieldDefinitions.MaxAsync(x => (int?)x.SortOrder) ?? 0;
        var sort = dto.SortOrder ?? maxOrder + 1;

        var entity = new InterventionWizardFieldDefinition
        {
            Key = key,
            Label = dto.Label.Trim(),
            FieldType = ft,
            SortOrder = sort,
            IsRequired = dto.IsRequired,
            CreationMode = mode,
            SourceSchema = mode == InterventionWizardFieldCreationMode.DatabaseBinding ? srcSchema : null,
            SourceTable = mode == InterventionWizardFieldCreationMode.DatabaseBinding ? srcTable : null,
            SourceColumn = mode == InterventionWizardFieldCreationMode.DatabaseBinding ? srcColumn : null,
            OptionsJson = SerializeOptionsForCreate(mode, ft, dto.FieldOptions),
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.InterventionWizardFieldDefinitions.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(MapToDto(entity));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InterventionWizardFieldDefinitionDto>> Update(
        int id,
        [FromBody] InterventionWizardFieldUpdateDto dto,
        CancellationToken ct)
    {
        var entity = await _db.InterventionWizardFieldDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Label))
            return BadRequest("Label is required.");

        var ft = Enum.IsDefined(typeof(InterventionWizardFieldType), dto.FieldType)
            ? (InterventionWizardFieldType)dto.FieldType
            : InterventionWizardFieldType.Text;

        var mode = dto.CreationMode == (int)InterventionWizardFieldCreationMode.DatabaseBinding
            ? InterventionWizardFieldCreationMode.DatabaseBinding
            : InterventionWizardFieldCreationMode.CustomField;

        string? srcSchema = null;
        string? srcTable = null;
        string? srcColumn = null;

        if (mode == InterventionWizardFieldCreationMode.DatabaseBinding)
        {
            srcSchema = string.IsNullOrWhiteSpace(dto.SourceSchema) ? "dbo" : dto.SourceSchema!.Trim();
            srcTable = dto.SourceTable?.Trim() ?? "";
            srcColumn = dto.SourceColumn?.Trim() ?? "";
            if (!SqlIdentifier.IsSafePart(srcSchema) || !SqlIdentifier.IsSafePart(srcTable) || !SqlIdentifier.IsSafePart(srcColumn))
                return BadRequest("Invalid schema, table or column name.");
            if (ft != InterventionWizardFieldType.Select)
                return BadRequest("A field linked to the database must use type « Selection (dropdown) ».");

            await _db.Database.OpenConnectionAsync(ct);
            try
            {
                var connection = _db.Database.GetDbConnection();
                if (!await SqlIdentifier.ColumnExistsAsync(connection, srcSchema, srcTable, srcColumn, ct))
                    return BadRequest("Table or column not found in the database.");
            }
            finally
            {
                await _db.Database.CloseConnectionAsync();
            }
        }
        else if (ft == InterventionWizardFieldType.Select)
        {
            var opt = SerializeOptions(dto.FieldOptions);
            if (string.IsNullOrWhiteSpace(opt))
                return BadRequest("Add at least one choice for a selection field.");
        }

        entity.Label = dto.Label.Trim();
        entity.FieldType = ft;
        entity.IsRequired = dto.IsRequired;
        entity.SortOrder = dto.SortOrder;
        entity.CreationMode = mode;
        entity.SourceSchema = mode == InterventionWizardFieldCreationMode.DatabaseBinding ? srcSchema : null;
        entity.SourceTable = mode == InterventionWizardFieldCreationMode.DatabaseBinding ? srcTable : null;
        entity.SourceColumn = mode == InterventionWizardFieldCreationMode.DatabaseBinding ? srcColumn : null;
        entity.OptionsJson = SerializeOptionsForUpdate(mode, ft, dto.FieldOptions);

        await _db.SaveChangesAsync();

        return Ok(MapToDto(entity));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.InterventionWizardFieldDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();

        _db.InterventionWizardFieldDefinitions.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static InterventionWizardFieldDefinitionDto MapToDto(InterventionWizardFieldDefinition x) => new()
    {
        Id = x.Id,
        Key = x.Key,
        Label = x.Label,
        FieldType = (int)x.FieldType,
        SortOrder = x.SortOrder,
        IsRequired = x.IsRequired,
        CreationMode = (int)x.CreationMode,
        SourceSchema = x.SourceSchema,
        SourceTable = x.SourceTable,
        SourceColumn = x.SourceColumn,
        Options = DeserializeOptions(x.OptionsJson)
    };

    private static string? SerializeOptionsForCreate(
        InterventionWizardFieldCreationMode mode,
        InterventionWizardFieldType ft,
        List<string>? fieldOptions)
    {
        if (ft != InterventionWizardFieldType.Select) return null;
        if (mode == InterventionWizardFieldCreationMode.DatabaseBinding) return null;
        return SerializeOptions(fieldOptions);
    }

    private static string? SerializeOptionsForUpdate(
        InterventionWizardFieldCreationMode mode,
        InterventionWizardFieldType ft,
        List<string>? fieldOptions)
    {
        if (ft != InterventionWizardFieldType.Select) return null;
        if (mode == InterventionWizardFieldCreationMode.DatabaseBinding) return null;
        return SerializeOptions(fieldOptions);
    }

    private async Task<string> EnsureUniqueKeyAsync(string baseKey)
    {
        var key = baseKey;
        var n = 0;
        while (await _db.InterventionWizardFieldDefinitions.AnyAsync(x => x.Key == key))
        {
            n++;
            key = $"{baseKey}_{n}";
        }
        return key;
    }

    private static string? SerializeOptions(List<string>? options)
    {
        if (options is null || options.Count == 0)
            return null;
        var cleaned = options
            .Select(o => o.Trim())
            .Where(o => o.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();
        return cleaned.Count == 0 ? null : JsonSerializer.Serialize(cleaned);
    }

    private static string[]? DeserializeOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            return JsonSerializer.Deserialize<string[]>(json);
        }
        catch
        {
            return null;
        }
    }

    private static string Slugify(string label)
    {
        var s = label.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in s)
        {
            if (char.GetUnicodeCategory(ch) == System.Globalization.UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else if (char.IsWhiteSpace(ch) || ch is '-' or '_') sb.Append('_');
        }
        var raw = Regex.Replace(sb.ToString().Trim('_'), "_+", "_");
        return string.IsNullOrEmpty(raw) ? "field" : raw;
    }
}
