namespace robot_controller_api.Exceptions;

public class DuplicateNameException : Exception
{
    public DuplicateNameException(string name)
        : base($"An entity with the name '{name}' already exists.")
    {
    }
}

public class NotFoundException : Exception
{
    public NotFoundException(int id) 
        : base($"An entity with ID {id} was not found.")
    {
    }
}