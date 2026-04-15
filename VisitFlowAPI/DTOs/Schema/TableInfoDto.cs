namespace VisitFlowAPI.DTOs.Schema;

public record TableInfoDto(string SchemaName, string TableName);

public record ColumnInfoDto(string ColumnName, string DataType);
