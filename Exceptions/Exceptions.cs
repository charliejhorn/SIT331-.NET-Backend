namespace robot_controller_api.Exceptions;

public class DuplicateCommandNameException : Exception
{
    public DuplicateCommandNameException(string name)
        : base($"A command with the name '{name}' already exists.")
    {
    }
}

public class CommandNotFoundException : Exception
{
    public CommandNotFoundException(int id) 
        : base($"Command with ID {id} was not found.")
    {
    }
}