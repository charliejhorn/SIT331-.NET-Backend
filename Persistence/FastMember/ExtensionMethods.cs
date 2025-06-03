using System.Text;
using FastMember;
using Npgsql;

namespace robot_controller_api.Persistence;

public static class ExtensionMethods
{
    public static void MapTo<T>(this NpgsqlDataReader dr, T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // create a fast type accessor to iterate through properties. entity here might be a RobotCommand or a Map
        TypeAccessor fastMember = TypeAccessor.Create(entity.GetType()); 

        // adds property names of the members of the entity into a HashSet
        HashSet<string> props = fastMember.GetMembers().Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // loop through sql columns
        for (int i = 0; i < dr.FieldCount; i++)
        {
            string columnName = dr.GetName(i);
            
            // convert snake_case column name to PascalCase for C# property matching
            string pascalCaseColumnName = ConvertSnakeCaseToPascalCase(columnName);
            
            // search for corresponding property name in entity/class definition
            // try both original column name and converted PascalCase name
            string? prop = props.FirstOrDefault(x => x.Equals(columnName, StringComparison.OrdinalIgnoreCase)) 
                          ?? props.FirstOrDefault(x => x.Equals(pascalCaseColumnName, StringComparison.OrdinalIgnoreCase));

            // additional null check because db and c# nulls are not the same
            if (!string.IsNullOrEmpty(prop)) 
                fastMember[entity, prop] = dr.IsDBNull(i) ? null : dr.GetValue(i);
        }
    }

    /// <summary>
    /// converts snake_case string to PascalCase
    /// example: "first_name" becomes "FirstName"
    /// </summary>
    /// <param name="snakeCase">the snake_case string to convert</param>
    /// <returns>PascalCase string</returns>
    private static string ConvertSnakeCaseToPascalCase(string snakeCase)
    {
        if (string.IsNullOrEmpty(snakeCase))
            return snakeCase;

        // split by underscore and capitalize each word
        string[] words = snakeCase.Split('_', StringSplitOptions.RemoveEmptyEntries);
        StringBuilder result = new StringBuilder();

        foreach (string word in words)
        {
            if (word.Length > 0)
            {
                // capitalize first letter and append rest of word in lowercase
                result.Append(char.ToUpper(word[0]));
                if (word.Length > 1)
                    result.Append(word.Substring(1).ToLower());
            }
        }

        return result.ToString();
    }
}