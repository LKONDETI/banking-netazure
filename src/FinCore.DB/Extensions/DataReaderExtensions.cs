using System.Data;

namespace FinCore.DB.Extensions;

public static class DataReaderExtensions
{
    public static string GetStringSafe(this IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    public static decimal GetDecimalSafe(this IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0m : reader.GetDecimal(ordinal);
    }

    public static int GetInt32Safe(this IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }

    public static bool GetBooleanSafe(this IDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal))
            return false;

        // DB2 may store booleans as CHAR('Y'/'N') or SMALLINT(1/0)
        var fieldType = reader.GetFieldType(ordinal);
        if (fieldType == typeof(string))
        {
            var value = reader.GetString(ordinal).Trim().ToUpperInvariant();
            return value is "Y" or "1" or "TRUE";
        }

        return reader.GetBoolean(ordinal);
    }

    public static T GetEnumSafe<T>(this IDataReader reader, string columnName, T defaultValue)
        where T : struct, Enum
    {
        var raw = reader.GetStringSafe(columnName);
        return Enum.TryParse<T>(raw, ignoreCase: true, out var result) ? result : defaultValue;
    }
}
