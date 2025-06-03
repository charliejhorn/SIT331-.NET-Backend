using Npgsql;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;

namespace robot_controller_api.Persistence;

public class RobotCommandRepository : IRobotCommandDataAccess, IRepository
{
    private IRepository _repo => this;
    public List<RobotCommand> GetRobotCommands()
    {
        List<RobotCommand> commands = _repo.ExecuteReader<RobotCommand>("SELECT * FROM robot_command");
        return commands;
    }

    public List<RobotCommand> GetMoveRobotCommands()
    {
        List<RobotCommand> commands = _repo.ExecuteReader<RobotCommand>("SELECT * FROM robot_command WHERE is_move_command = TRUE");
        return commands;
    }

    public RobotCommand GetRobotCommandById(int id)
    {
        string sqlCommand = "SELECT * FROM robot_command WHERE id = $1;";
        NpgsqlParameter[] sqlParams = [new() { Value = id }];
        RobotCommand? result = _repo.ExecuteReader<RobotCommand>(sqlCommand, sqlParams).SingleOrDefault();
        if (result == null)
            throw new NotFoundException(id);
        return result;
    }

    public RobotCommand AddRobotCommand(RobotCommand newCommand)
    {
        // check if command name already exists
        string checkCommand = "SELECT * FROM robot_command WHERE name = $1";
        NpgsqlParameter[] checkParams = [ new() { Value = newCommand.Name } ];
        
        List<RobotCommand> existingCommands = _repo.ExecuteReader<RobotCommand>(checkCommand, checkParams);
        if (existingCommands.Any())
            throw new DuplicateNameException(newCommand.Name);


        // insert new command
        string sqlCommand = "INSERT INTO robot_command (name, description, is_move_command, created_date, modified_date) VALUES(($1), ($2), ($3), ($4), ($5)) RETURNING *;";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = newCommand.Name },
            new() { Value = newCommand.Description == null ? DBNull.Value : newCommand.Description },
            new() { Value = newCommand.IsMoveCommand },
            new() { Value = DateTime.Now },
            new() { Value = DateTime.Now }
        };
        RobotCommand result = _repo.ExecuteReader<RobotCommand>(sqlCommand, sqlParams).Single();
        return result;
    }

    public RobotCommand UpdateRobotCommand(int id, RobotCommand updatedCommand)
    {
        // check if command exists first
        RobotCommand existingCommand = GetRobotCommandById(id); // this will throw if not found
        
        // check for duplicate name
        string duplicateCheck = "SELECT * FROM robot_command WHERE name = $1 AND id != $2";
        NpgsqlParameter[] checkParams = new NpgsqlParameter[] {
            new() { Value = updatedCommand.Name },
            new() { Value = id }
        };
        
        List<RobotCommand> duplicates = _repo.ExecuteReader<RobotCommand>(duplicateCheck, checkParams);
        if (duplicates.Any())
            throw new DuplicateNameException(updatedCommand.Name);
        
        // update the command
        string sqlCommand = "UPDATE robot_command SET name=($1), description=($2), is_move_command=($3), modified_date=($4) WHERE id=($5) RETURNING *;";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = updatedCommand.Name },
            new() { Value = updatedCommand.Description == null ? DBNull.Value : updatedCommand.Description },
            new() { Value = updatedCommand.IsMoveCommand },
            new() { Value = DateTime.Now },
            new() { Value = id }
        };
        RobotCommand result = _repo.ExecuteReader<RobotCommand>(sqlCommand, sqlParams).Single();
        return result;
    }

    public bool DeleteRobotCommand(int id)
    {
        // check if command exists
        RobotCommand existingCommand = GetRobotCommandById(id); // this will throw error if it doesn't

        string sqlCommand = "DELETE FROM robot_command WHERE id=($1)";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() {Value = id}
        };
        int affectedRows = _repo.ExecuteNonQuery(sqlCommand, sqlParams);
        return affectedRows > 0;
    }
}