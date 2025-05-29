using FastMember;
using Npgsql;

namespace robot_controller_api.Persistence;

public static class ExtensionMethods
{
    public static void MapTo<T>(this NpgsqlDataReader dr, T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // create a fast type accessor to iterate through properties. entity here might be a RobotCommand or a Map
        var fastMember = TypeAccessor.Create(entity.GetType()); 

        // adds property names of the members of the entity into a HashSet
        var props = fastMember.GetMembers().Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // loop through sql columns
        for (int i = 0; i < dr.FieldCount; i++)
        {
            // search for corresponding property name in entity/class definition
            var prop = props.FirstOrDefault(x => x.Equals(dr.GetName(i), StringComparison.OrdinalIgnoreCase)); 

            // additional null check because db and c# nulls are not the same
            if (!string.IsNullOrEmpty(prop)) 
                fastMember[entity, prop] = dr.IsDBNull(i) ? null : dr.GetValue(i);
        }
    } 
}