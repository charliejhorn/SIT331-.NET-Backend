using Npgsql;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;

namespace robot_controller_api.Persistence;

public class MapRepository : IMapDataAccess, IRepository
{
    private IRepository _repo => this;

    public List<Map> GetMaps()
    {
        List<Map> maps = _repo.ExecuteReader<Map>("SELECT * FROM public.map");
        return maps;
    }

    public List<Map> GetSquareMaps()
    {
        List<Map> maps = _repo.ExecuteReader<Map>("SELECT * FROM public.map WHERE columns = rows");
        return maps;
    }

    public Map GetMapById(int id)
    {
        string sqlCommand = "SELECT * FROM map WHERE id = $1;";
        NpgsqlParameter[] sqlParams = [new() { Value = id }];
        Map? result = _repo.ExecuteReader<Map>(sqlCommand, sqlParams).SingleOrDefault();
        if (result == null)
            throw new NotFoundException(id);
        return result;
    }

    public Map AddMap(Map newMap)
    {
        // check if map name already exists
        string checkCommand = "SELECT * FROM map WHERE name = $1";
        NpgsqlParameter[] checkParams = [new() { Value = newMap.Name }];
        
        List<Map> existingMaps = _repo.ExecuteReader<Map>(checkCommand, checkParams);
        if (existingMaps.Any())
            throw new DuplicateNameException(newMap.Name);

        newMap.CreatedDate = DateTime.Now;
        newMap.ModifiedDate = DateTime.Now;

        string sqlCommand = "INSERT INTO map (columns, rows, name, description, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5), ($6)) RETURNING *;";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = newMap.Columns },
            new() { Value = newMap.Rows },
            new() { Value = newMap.Name },
            new() { Value = newMap.Description == null ? DBNull.Value : newMap.Description },
            new() { Value = newMap.CreatedDate },
            new() { Value = newMap.ModifiedDate }
        };
        Map result = _repo.ExecuteReader<Map>(sqlCommand, sqlParams).Single();
        return result;
    }

    public Map UpdateMap(int id, Map updatedMap)
    {
        // check if map exists first
        Map existingMap = GetMapById(id); // this will throw if not found
        
        // check for duplicate name
        string duplicateCheck = "SELECT * FROM map WHERE name = $1 AND id != $2";
        NpgsqlParameter[] checkParams = new NpgsqlParameter[] {
            new() { Value = updatedMap.Name },
            new() { Value = id }
        };
        
        List<Map> duplicates = _repo.ExecuteReader<Map>(duplicateCheck, checkParams);
        if (duplicates.Any())
            throw new DuplicateNameException(updatedMap.Name);
        
        // update the map
        string sqlCommand = "UPDATE map SET columns=($1), rows=($2), name=($3), description=($4), modifieddate=($5) WHERE id=($6) RETURNING *;";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = updatedMap.Columns },
            new() { Value = updatedMap.Rows },
            new() { Value = updatedMap.Name },
            new() { Value = updatedMap.Description == null ? DBNull.Value : updatedMap.Description },
            new() { Value = DateTime.Now },
            new() { Value = id }
        };
        Map result = _repo.ExecuteReader<Map>(sqlCommand, sqlParams).Single();
        return result;
    }

    public bool DeleteMap(int id)
    {
        // check if map exists first
        Map existingMap = GetMapById(id); // this will throw if not found

        string sqlCommand = "DELETE FROM map WHERE id=($1)";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() {Value = id}
        };
        int affectedRows = _repo.ExecuteNonQuery(sqlCommand, sqlParams);
        
        // ensure exactly one row was deleted
        if (affectedRows != 1)
        {
            throw new InvalidOperationException($"Expected to delete 1 row, but {affectedRows} rows were affected.");
        }
        
        return true;
    }
}