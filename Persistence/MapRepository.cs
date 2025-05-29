using Npgsql;

namespace robot_controller_api.Persistence;

public class MapRepository : IMapDataAccess, IRepository
{
    private IRepository _repo => this;

    public List<Map> GetMaps()
    {
        var maps = _repo.ExecuteReader<Map>("SELECT * FROM public.map");
        return maps;
    }

    public Map AddMap(Map newMap)
    {
        var sqlCommand = "INSERT INTO map (columns, rows, name, description, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5), ($6)) RETURNING *;";
        var sqlParams = new NpgsqlParameter[]{
            new() { Value = newMap.Columns },
            new() { Value = newMap.Rows },
            new() { Value = newMap.Name },
            new() { Value = newMap.Description == null ? DBNull.Value : newMap.Description },
            new() { Value = newMap.CreatedDate },
            new() { Value = newMap.ModifiedDate }
        };
        var result = _repo.ExecuteReader<Map>(sqlCommand, sqlParams).Single();
        return result;
    }

    public Map UpdateMap(Map updatedMap)
    {
        var sqlCommand = "UPDATE map SET columns=($1), rows=($2), name=($3), description=($4), modifieddate=($5) WHERE id=($6) RETURNING *;";
        var sqlParams = new NpgsqlParameter[]{
            new() { Value = updatedMap.Columns },
            new() { Value = updatedMap.Rows },
            new() { Value = updatedMap.Name },
            new() { Value = updatedMap.Description == null ? DBNull.Value : updatedMap.Description },
            new() { Value = updatedMap.ModifiedDate },
            new() { Value = updatedMap.Id }
        };
        var result = _repo.ExecuteReader<Map>(sqlCommand, sqlParams).Single();
        return result;
    }

    public bool DeleteMap(int id)
    {
        var sqlCommand = "DELETE FROM map WHERE id=($1)";
        var sqlParams = new NpgsqlParameter[]{
            new() {Value = id}
        };
        var affectedRows = _repo.ExecuteNonQuery(sqlCommand, sqlParams);
        return affectedRows > 0;
    }
}