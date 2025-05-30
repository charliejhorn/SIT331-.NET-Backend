using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public interface IMapDataAccess
{
    List<Map> GetMaps();
    List<Map> GetSquareMaps();
    Map GetMapById(int id);
    Map AddMap(Map newMap);
    Map UpdateMap(int id, Map updatedMap);
    bool DeleteMap(int id);
}