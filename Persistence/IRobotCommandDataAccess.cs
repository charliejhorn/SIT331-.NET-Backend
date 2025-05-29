using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public interface IRobotCommandDataAccess
{
    List<RobotCommand> GetRobotCommands();
    RobotCommand? AddRobotCommand(RobotCommand newCommand);
    RobotCommand? UpdateRobotCommand(RobotCommand updatedCommand);
    bool DeleteRobotCommand(int id);
}