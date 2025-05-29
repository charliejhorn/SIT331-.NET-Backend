using Npgsql;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class RobotCommandRepository : IRobotCommandDataAccess, IRepository
{
    private IRepository _repo => this;
    public List<RobotCommand> GetRobotCommands()
    {
        var commands = _repo.ExecuteReader<RobotCommand>("SELECT * FROM public.robotcommand");
        return commands;
    }

    public RobotCommand AddRobotCommand(RobotCommand newCommand)
    {
        var sqlCommand = "INSERT INTO robotcommand (name, description, ismovecommand, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5)) RETURNING *;";
        var sqlParams = new NpgsqlParameter[]{
            new() { Value = newCommand.Name },
            new() { Value = newCommand.Description == null ? DBNull.Value : newCommand.Description },
            new() { Value = newCommand.IsMoveCommand },
            new() { Value = newCommand.CreatedDate },
            new() { Value = newCommand.ModifiedDate }
        };
        var result = _repo.ExecuteReader<RobotCommand>(sqlCommand, sqlParams).Single();
        return result;
    }

    public RobotCommand UpdateRobotCommand(RobotCommand updatedCommand)
    {
        var sqlCommand = "UPDATE robotcommand SET name=($1), description=($2), ismovecommand=($3), modifieddate=($4) WHERE id=($5) RETURNING *;";
        var sqlParams = new NpgsqlParameter[]{
            new() { Value = updatedCommand.Name },
            new() { Value = updatedCommand.Description == null ? DBNull.Value : updatedCommand.Description },
            new() { Value = updatedCommand.IsMoveCommand },
            new() { Value = updatedCommand.ModifiedDate },
            new() { Value = updatedCommand.Id }
        };
        var result = _repo.ExecuteReader<RobotCommand>(sqlCommand, sqlParams).Single();
        return result;
    }

    public bool DeleteRobotCommand(int id)
    {
        var sqlCommand = "DELETE FROM robotcommand WHERE id=($1)";
        var sqlParams = new NpgsqlParameter[]{
            new() {Value = id}
        };
        var affectedRows = _repo.ExecuteNonQuery(sqlCommand, sqlParams);
        return affectedRows > 0;
    }
}