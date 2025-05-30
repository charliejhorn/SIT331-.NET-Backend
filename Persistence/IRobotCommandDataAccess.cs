using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public interface IRobotCommandDataAccess
{
    List<RobotCommand> GetRobotCommands();
    List<RobotCommand> GetMoveRobotCommands();
    RobotCommand GetRobotCommandById(int id);
    RobotCommand AddRobotCommand(RobotCommand newCommand);
    RobotCommand UpdateRobotCommand(int id, RobotCommand updatedCommand);
    bool DeleteRobotCommand(int id);
}