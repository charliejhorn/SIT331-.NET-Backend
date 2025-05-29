using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public interface IMapDataAccess
{
    List<Map> GetMaps();
    Map? AddMap(Map newMap);
    Map? UpdateMap(Map updatedMap);
    bool DeleteMap(int id);
}