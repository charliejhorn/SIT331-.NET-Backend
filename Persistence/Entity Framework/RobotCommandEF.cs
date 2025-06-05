using Microsoft.EntityFrameworkCore;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;
using robot_controller_api.Contexts;

namespace robot_controller_api.Persistence;

public class RobotCommandEF : IRobotCommandDataAccess
{
    private readonly RobotContext _context;

    public RobotCommandEF(RobotContext context)
    {
        _context = context;
    }

    public List<RobotCommand> GetRobotCommands()
    {
        return _context.RobotCommands.ToList();
    }

    public List<RobotCommand> GetMoveRobotCommands()
    {
        return _context.RobotCommands.Where(command => command.IsMoveCommand == true).ToList();
    }

    public RobotCommand GetRobotCommandById(int id)
    {
        RobotCommand? command = _context.RobotCommands.Find(id);
        if (command == null)
            throw new NotFoundException(id);
        return command;
    }

    public RobotCommand AddRobotCommand(RobotCommand newCommand)
    {
        // check if command name already exists
        bool nameExists = _context.RobotCommands.Any(command => command.Name == newCommand.Name);
        if (nameExists)
            throw new DuplicateNameException(newCommand.Name);

        // Use DateTime.UtcNow to avoid timezone serialization issues
        var utcNow = DateTime.UtcNow;
        newCommand.CreatedDate = utcNow;
        newCommand.ModifiedDate = utcNow;

        _context.RobotCommands.Add(newCommand);
        _context.SaveChanges();
        return newCommand;
    }

    public RobotCommand UpdateRobotCommand(int id, RobotCommand inputCommand)
    {
        // check name doesn't already exist
        bool nameExists = _context.RobotCommands.Any(command => command.Id != id && command.Name == inputCommand.Name);
        if (nameExists)
            throw new DuplicateNameException(inputCommand.Name);

        // find command
        RobotCommand? target = _context.RobotCommands.Find(id);

        // check command exists
        if (target == null)
            throw new NotFoundException(id);

        // update properties
        target.Name = inputCommand.Name;
        target.Description = inputCommand.Description;
        target.IsMoveCommand = inputCommand.IsMoveCommand;
        target.ModifiedDate = DateTime.UtcNow;

        // push changes to db and return the updated command
        _context.SaveChanges();
        return target;
    }

    public bool DeleteRobotCommand(int id)
    {
        // check if command exists first
        RobotCommand existingCommand = GetRobotCommandById(id); // this will throw if not found

        _context.RobotCommands.Remove(existingCommand);
        int affectedRows = _context.SaveChanges();
        
        // ensure exactly one row was deleted
        if (affectedRows != 1)
        {
            throw new InvalidOperationException($"Expected to delete 1 row, but {affectedRows} rows were affected.");
        }
        
        return true;
    }
}