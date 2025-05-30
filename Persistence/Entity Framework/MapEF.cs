using Microsoft.EntityFrameworkCore;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;

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

    public List<Map> GetSquareMaps()
    {
        return _context.Maps.Where(map => map.Columns == map.Rows).ToList();
    }

    public Map GetMapById(int id)
    {
        Map? map = _context.Maps.Find(id);
        if (map == null)
            throw new NotFoundException(id);
        return map;
    }

    public Map AddMap(Map newMap)
    {
        // check if map name already exists
        bool nameExists = _context.Maps.Any(map => map.Name == newMap.Name);
        if (nameExists)
            throw new DuplicateNameException(newMap.Name);

        newMap.CreatedDate = DateTime.Now;
        newMap.ModifiedDate = DateTime.Now;

        _context.Maps.Add(newMap);
        _context.SaveChanges();
        return newMap;
    }

    public Map UpdateMap(int id, Map updatedMap)
    {
        // check if map name already exists (excluding current map)
        bool nameExists = _context.Maps.Any(map => map.Id != id && map.Name == updatedMap.Name);
        if (nameExists)
            throw new DuplicateNameException(updatedMap.Name);

        // find map
        Map? target = _context.Maps.Find(id);

        // check map exists
        if (target == null)
            throw new NotFoundException(id);

        // update properties
        target.Name = updatedMap.Name;
        target.Description = updatedMap.Description;
        target.Columns = updatedMap.Columns;
        target.Rows = updatedMap.Rows;
        target.ModifiedDate = DateTime.Now;

        // push changes to db and return the updated map
        _context.SaveChanges();
        return target;
    }

    public bool DeleteMap(int id)
    {
        // check if map exists first
        Map existingMap = GetMapById(id); // this will throw if not found

        _context.Maps.Remove(existingMap);
        int affectedRows = _context.SaveChanges();
        return affectedRows > 0;
    }
}
