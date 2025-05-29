using Microsoft.EntityFrameworkCore;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class MapEF : IMapDataAccess
{
    private readonly RobotContext _context;

    public MapEF(RobotContext context)
    {
        _context = context;
    }

    public List<Map> GetMaps()
    {
        return _context.Maps.ToList();
    }

    public Map? AddMap(Map newMap)
    {
        _context.Maps.Add(newMap);
        _context.SaveChanges();
        return newMap;
    }

    public Map? UpdateMap(Map updatedMap)
    {
        _context.Entry(updatedMap).State = EntityState.Modified;
        _context.SaveChanges();
        return updatedMap;
    }

    public bool DeleteMap(int id)
    {
        var map = _context.Maps.Find(id);
        if (map == null)
        {
            return false;
        }

        _context.Maps.Remove(map);
        var affectedRows = _context.SaveChanges();
        return affectedRows > 0;
    }
}
