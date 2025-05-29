namespace robot_controller_api;

public class Map
{
    public int Id {get; set;}
    public int Columns {get; set;}
    public int Rows {get; set;}
    public bool IsSquare {get; set;}
    public string? Name {get; set;}
    public string? Description {get; set;}
    public DateTime CreatedDate {get; set;}
    public DateTime ModifiedDate {get; set;}

    // default constructor
    public Map(int id, int columns, int rows, string name, DateTime createdDate, DateTime modifiedDate, string? description = null)
    {
        Id = id;
        Columns = columns;
        Rows = rows;
        IsSquare = rows == columns;
        Name = name;
        CreatedDate = createdDate;
        ModifiedDate = modifiedDate;
        Description = description;
    }

    public Map()
    {
        Id = default;
        Columns = default;
        Rows = default;
        IsSquare = default;
        Name = default;
        CreatedDate = default;
        ModifiedDate = default;
        Description = default;
    }

    public override string ToString()
    {
        return $"Map: {Id}, {Columns}, {Rows}, {Name}, {CreatedDate}, {ModifiedDate}, {Description}";
    }
}
