using Npgsql;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;

namespace robot_controller_api.Persistence;

public class MapADO : IMapDataAccess
{
    // Connection string is usually set in a config file for the ease of change.
    private const string CONNECTION_STRING = "Host=localhost;Username=cjhorn;Password=;Database=sit331";

    public List<Map> GetMaps()
    {
        List<Map> Maps = new List<Map>();
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM map", conn);
        using NpgsqlDataReader dr = cmd.ExecuteReader();
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

    public List<Map> GetSquareMaps()
    {
        List<Map> Maps = new List<Map>();
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM map WHERE columns = rows", conn);
        using NpgsqlDataReader dr = cmd.ExecuteReader();
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

    public Map GetMapById(int id)
    {
        Map? map = null;
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM map WHERE id = $1", conn)
        {
            Parameters = { new() { Value = id } }
        };
        using NpgsqlDataReader dr = cmd.ExecuteReader();
        if (dr.Read())
        {
            // read values off the data reader and create a new Map
            int mapId = (int)dr["id"];
            int columns = (int)dr["columns"];
            int rows = (int)dr["rows"];
            string name = (string)dr["name"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            map = new Map(mapId, columns, rows, name, createdDate, modifiedDate, description);
        }
        if (map == null)
            throw new NotFoundException(id);

        return map;
    }

    public Map AddMap(Map newMap)
    {
        // check if map name already exists
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        
        using NpgsqlCommand checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM map WHERE name = $1", conn)
        {
            Parameters = { new() { Value = newMap.Name } }
        };
        
        int nameCount = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (nameCount > 0)
            throw new DuplicateNameException(newMap.Name);

        newMap.CreatedDate = DateTime.Now;
        newMap.ModifiedDate = DateTime.Now;

        using NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO map (columns, rows, name, description, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5), ($6)) RETURNING *;", conn)
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

        using NpgsqlDataReader dr = cmd.ExecuteReader();

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

        if (returnedMap == null)
            throw new Exception("Map could not be created.");

        return returnedMap;
    }

    public Map UpdateMap(int id, Map inputMap)
    {
        List<Map> maps = GetMaps();

        // check map exists
        Map? existingMap = maps.Find(map => map.Id == id);
        if (existingMap == null)
            throw new NotFoundException(id);

        // check name doesn't already exist
        if(maps.Exists(map => map.Id != id && map.Name == inputMap.Name))
            throw new DuplicateNameException(inputMap.Name);

        // send db query
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("UPDATE map SET columns=($1), rows=($2), name=($3), description=($4), modifieddate=($5) WHERE id=($6) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = inputMap.Columns },
                new() { Value = inputMap.Rows },
                new() { Value = inputMap.Name },
                new() { Value = inputMap.Description == null ? DBNull.Value : inputMap.Description },
                new() { Value = DateTime.Now },
                new() { Value = id }
            }
        };

        // read returned map
        using NpgsqlDataReader dr = cmd.ExecuteReader();
        Map? returnedMap = null;
        while (dr.Read())
        {
            // read values off the data reader and create a new Map
            int mapId = (int)dr["id"];
            int columns = (int)dr["columns"];
            int rows = (int)dr["rows"];
            string name = (string)dr["name"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            returnedMap = new Map(mapId, columns, rows, name, createdDate, modifiedDate, description);
        }

        if (returnedMap == null)
            throw new Exception("Map could not be updated.");

        return returnedMap;
    }

    public bool DeleteMap(int id)
    {
        // check if map exists first
        Map existingMap = GetMapById(id); // this will throw if not found

        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using NpgsqlCommand cmd = new NpgsqlCommand("DELETE FROM map WHERE id=($1)", conn)
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