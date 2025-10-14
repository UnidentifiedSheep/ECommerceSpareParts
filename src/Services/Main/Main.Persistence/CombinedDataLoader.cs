using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Main.Core.Interfaces;
using Main.Core.Models;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using Persistence.Extensions;

namespace Main.Persistence;

public class CombinedDataLoader(DContext context) : ICombinedDataLoader
{
    private static string GetKey(string field, int index) => $"{field}_{index}";

    public async Task<IReadOnlyList<(ExistenceCheck rule, object[] foundValues)>> GetExistenceChecks(
        IEnumerable<ExistenceCheck> rules, CancellationToken cancellationToken = default)
    {
        var rulesList = rules.ToList();
        var krDict = new Dictionary<string, ExistenceCheck>();

        var sql = BuildSql(rulesList, krDict);
        var result = new List<(ExistenceCheck rule, object[] foundValues)>();
        

        var connection = context.Database.GetDbConnection();
        
        var hasActiveTransaction = context.Database.CurrentTransaction != null;
        var openedHere = false;
        
        if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
        {
            await connection.OpenAsync(cancellationToken);
            openedHere = true;
        }

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = CommandType.Text;

            int idx = 0;
            foreach (var rule in rulesList)
            {
                var param = CreateParameter($"@p{idx++}", rule.Keys.ToArray(), rule.KeyType);
                command.Parameters.Add(param);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return result;

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var colName = reader.GetName(i);
                var value = reader.GetValue(i) is Array npgsqlArray
                    ? npgsqlArray.Cast<object>().ToArray()
                    : [];

                var rule = krDict[colName];
                result.Add((rule, value));
            }

            return result;
        }
        finally
        {
            if (!hasActiveTransaction && openedHere && connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
        
    }

    private string BuildSql(IEnumerable<ExistenceCheck> rules, Dictionary<string, ExistenceCheck> ruleMap)
    {
        var sb = new StringBuilder("SELECT ");
        int ruleIdx = 0;

        foreach (var rule in rules)
        {
            var fieldName = context.GetColumnName(rule.EntityType, rule.KeySelector);
            var tableName = context.GetTableName(rule.EntityType);
            var schema = context.GetSchemaName(rule.EntityType);

            var varName = GetKey(fieldName, ruleIdx);
            ruleMap[varName] = rule;

            sb.AppendLine($"""
                ARRAY(
                    SELECT {fieldName}
                    FROM "{schema}"."{tableName}"
                    WHERE {fieldName} = ANY(@p{ruleIdx})
                ) AS "{varName}",
            """);

            ruleIdx++;
        }

        sb.Length -= 2; // remove last comma
        return sb.ToString();
    }

    private static DbParameter CreateParameter(string name, object[] keys, Type keyType)
    {
        var param = new NpgsqlParameter(name, ConvertToTypedArray(keys, keyType))
        {
            NpgsqlDbType = keyType switch
            {
                var t when t == typeof(Guid) => NpgsqlDbType.Uuid | NpgsqlDbType.Array,
                var t when t == typeof(int) => NpgsqlDbType.Integer | NpgsqlDbType.Array,
                var t when t == typeof(long) => NpgsqlDbType.Bigint | NpgsqlDbType.Array,
                var t when t == typeof(bool) => NpgsqlDbType.Boolean | NpgsqlDbType.Array,
                var t when t == typeof(DateTime) => NpgsqlDbType.Timestamp | NpgsqlDbType.Array,
                _ => NpgsqlDbType.Text | NpgsqlDbType.Array
            }
        };

        return param;
    }

    private static Array ConvertToTypedArray(object[] keys, Type elementType)
    {
        var arr = Array.CreateInstance(elementType, keys.Length);
        for (int i = 0; i < keys.Length; i++)
            arr.SetValue(keys[i], i);
        return arr;
    }
}