using Npgsql;

namespace PlantProduction.Api.Common;

public static class DbReaderExtensions
{
    public static int GetInt32(this NpgsqlDataReader reader, string column)
    {
        return reader.GetInt32(reader.GetOrdinal(column));
    }

    public static long GetInt64(this NpgsqlDataReader reader, string column)
    {
        return reader.GetInt64(reader.GetOrdinal(column));
    }

    public static string GetString(this NpgsqlDataReader reader, string column)
    {
        return reader.GetString(reader.GetOrdinal(column));
    }

    public static string? GetNullableString(this NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    public static int? GetNullableInt32(this NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    public static long? GetNullableInt64(this NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    public static decimal GetDecimal(this NpgsqlDataReader reader, string column)
    {
        return reader.GetDecimal(reader.GetOrdinal(column));
    }

    public static decimal? GetNullableDecimal(this NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDecimal(ordinal);
    }

    public static bool GetBoolean(this NpgsqlDataReader reader, string column)
    {
        return reader.GetBoolean(reader.GetOrdinal(column));
    }

    public static bool? GetNullableBoolean(this NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetBoolean(ordinal);
    }

    public static DateTime GetDateTime(this NpgsqlDataReader reader, string column)
    {
        return reader.GetDateTime(reader.GetOrdinal(column));
    }

    public static DateTime? GetNullableDateTime(this NpgsqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}
