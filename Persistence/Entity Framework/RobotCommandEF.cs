using Microsoft.EntityFrameworkCore;
using robot_controller_api.Models;

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
        return _context.RobotCommands.Where(command => command.IsMoveCommand).ToList();
    }

    public RobotCommand GetRobotCommandById(int id)
    {
        return _context.RobotCommands.Single(command => command.Id == id);
    }

    public RobotCommand AddRobotCommand(RobotCommand newCommand)
    {
        _context.RobotCommands.Add(newCommand);
        _context.SaveChanges();
        return newCommand;
    }

    public RobotCommand UpdateRobotCommand(RobotCommand updatedCommand)
    {
        // RobotCommand target = _context.RobotCommands.Single(command => command.Id == updatedCommand.Id);

        // target.

        // _context.Entry(updatedCommand).State = EntityState.Modified;
        _context.RobotCommands.Update(updatedCommand);

        // _context.ChangeTracker.DetectChanges();
        // Console.WriteLine(_context.ChangeTracker.DebugView.LongView);

        _context.SaveChanges();
        return updatedCommand;
    }

    public bool DeleteRobotCommand(int id)
    {
        var command = _context.RobotCommands.Find(id);
        if (command == null)
        {
            return false;
        }

        _context.RobotCommands.Remove(command);
        var affectedRows = _context.SaveChanges();
        return affectedRows > 0;
    }
}