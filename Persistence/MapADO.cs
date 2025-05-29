using Npgsql;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class MapADO : IMapDataAccess
{
    // Connection string is usually set in a config file for the ease of change.
    private const string CONNECTION_STRING = "Host=localhost;Username=cjhorn;Password=;Database=sit331";

    public List<Map> GetMaps()
    {
        var Maps = new List<Map>();
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT * FROM map", conn);
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            // read values off the data reader and create a new Map here and then add it to the result list.
            int id = (int)dr["id"];
            int columns = (int)dr["columns"];
            int rows = (int)dr["rows"];
            string name = (string)dr["name"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            Map map = new Map(id, columns, rows, name, createdDate, modifiedDate, description);
            Maps.Add(map);
        }
        return Maps;
    }

    public Map? AddMap(Map newMap)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand($"INSERT INTO map (columns, rows, name, description, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5), ($6)) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = newMap.Columns },
                new() { Value = newMap.Rows },
                new() { Value = newMap.Name },
                new() { Value = newMap.Description == null ? DBNull.Value : newMap.Description },
                new() { Value = newMap.CreatedDate },
                new() { Value = newMap.ModifiedDate }
            }
        };

        using var dr = cmd.ExecuteReader();

        Map? returnedMap = null;
        while (dr.Read())
        {
            // read values off the data reader and create a new Map here and then add it to the result list.
            int id = (int)dr["id"];
            int columns = (int)dr["columns"];
            int rows = (int)dr["rows"];
            string name = (string)dr["name"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            returnedMap = new Map(id, columns, rows, name, createdDate, modifiedDate, description);
        }

        return returnedMap;
    }

    public Map? UpdateMap(Map updatedMap)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand($"UPDATE map SET columns=($1), rows=($2), name=($3), description=($4), modifieddate=($5) WHERE id=($6) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = updatedMap.Columns },
                new() { Value = updatedMap.Rows },
                new() { Value = updatedMap.Name },
                new() { Value = updatedMap.Description == null ? DBNull.Value : updatedMap.Description },
                new() { Value = updatedMap.ModifiedDate },
                new() { Value = updatedMap.Id }
            }
        };

        using var dr = cmd.ExecuteReader();

        Map? returnedMap = null;
        while (dr.Read())
        {
            // read values off the data reader and create a new Map here and then add it to the result list.
            int id = (int)dr["id"];
            int columns = (int)dr["columns"];
            int rows = (int)dr["rows"];
            string name = (string)dr["name"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            returnedMap = new Map(id, columns, rows, name, createdDate, modifiedDate, description);
        }

        return returnedMap;
    }

    public bool DeleteMap(int id)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand($"DELETE FROM map WHERE id=($1)", conn)
        {
            Parameters =
            {
                new() { Value = id }
            }
        };

        int affectedRows = cmd.ExecuteNonQuery();

        if (affectedRows == 1) return true;
        else return false;
    }
}