using Npgsql;
using robot_controller_api.Exceptions;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class RobotCommandADO : IRobotCommandDataAccess
{
    // Connection string is usually set in a config file for the ease of change.
    private const string CONNECTION_STRING = "Host=localhost;Username=cjhorn;Password=;Database=sit331";

    public List<RobotCommand> GetRobotCommands()
    {
        List<RobotCommand> robotCommands = new List<RobotCommand>();
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT * FROM robotcommand", conn);
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            // read values off the data reader and create a new robotCommand here and then add it to the result list.
            int id = (int)dr["id"];
            string name = (string)dr["name"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            bool isMoveCommand = (bool)dr["ismovecommand"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            RobotCommand command = new RobotCommand(id, name, isMoveCommand, createdDate, modifiedDate, description);
            robotCommands.Add(command);
        }
        return robotCommands;
    }

    public List<RobotCommand> GetMoveRobotCommands()
    {
        List<RobotCommand> robotCommands = new List<RobotCommand>();
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT * FROM robotcommand WHERE ismovecommand = TRUE", conn);
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            // read values off the data reader and create a new robotCommand here and then add it to the result list.
            int id = (int)dr["id"];
            string name = (string)dr["name"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            bool isMoveCommand = (bool)dr["ismovecommand"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            RobotCommand command = new RobotCommand(id, name, isMoveCommand, createdDate, modifiedDate, description);
            robotCommands.Add(command);
        }
        return robotCommands;
    }

    public RobotCommand GetRobotCommandById(int id)
    {
        RobotCommand? command = null;
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT * FROM robotcommand WHERE id = $1", conn)
        {
            Parameters = { new() { Value = id } }
        };
        using var dr = cmd.ExecuteReader();
        if (dr.Read())
        {
            // read values off the data reader and create a new robotCommand here and then return it.
            int commandId = (int)dr["id"];
            string name = (string)dr["name"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            bool isMoveCommand = (bool)dr["ismovecommand"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            command = new RobotCommand(commandId, name, isMoveCommand, createdDate, modifiedDate, description);
        }
        if (command == null)
            throw new NotFoundException(id);

        return command;
    }

    public RobotCommand AddRobotCommand(RobotCommand newCommand)
    {
        // check if command name already exists
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        
        using var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM robotcommand WHERE name = $1", conn)
        {
            Parameters = { new() { Value = newCommand.Name } }
        };
        
        int nameCount = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (nameCount > 0)
            throw new DuplicateNameException(newCommand.Name);


        // insert new command
        using var cmd = new NpgsqlCommand("INSERT INTO robotcommand (name, description, ismovecommand, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5)) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = newCommand.Name },
                new() { Value = newCommand.Description == null ? DBNull.Value : newCommand.Description },
                new() { Value = newCommand.IsMoveCommand },
                new() { Value = DateTime.Now },
                new() { Value = DateTime.Now }
            }
        };

        using var dr = cmd.ExecuteReader();

        // read the new command that is returned from the query
        RobotCommand? returnedCommand = null;
        while (dr.Read())
        {
            // read values off the data reader and create a new robotCommand
            int id = (int)dr["id"];
            string name = (string)dr["name"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            bool isMoveCommand = (bool)dr["ismovecommand"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            returnedCommand = new RobotCommand(id, name, isMoveCommand, createdDate, modifiedDate, description);
        }
        
        if (returnedCommand == null)
            throw new Exception("Command could not be created.");
            
        return returnedCommand;
    }

    public RobotCommand UpdateRobotCommand(int id, RobotCommand inputCommand)
    {
        List<RobotCommand> commands = GetRobotCommands();

        // check command exists
        RobotCommand? existingCommand = commands.Find(command => command.Id == id);
        if (existingCommand == null)
            throw new NotFoundException(id);

        // check name doesn't already exist
        if(commands.Exists(command => command.Id != id && command.Name == inputCommand.Name))
            throw new DuplicateNameException(inputCommand.Name);

        // send db query
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using var cmd = new NpgsqlCommand("UPDATE robotcommand SET name=($1), description=($2), ismovecommand=($3), modifieddate=($4) WHERE id=($5) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = inputCommand.Name },
                new() { Value = inputCommand.Description == null ? DBNull.Value : inputCommand.Description },
                new() { Value = inputCommand.IsMoveCommand },
                new() { Value = DateTime.Now },
                new() { Value = id }
            }
        };

        // read returned command
        using var dr = cmd.ExecuteReader();
        RobotCommand? returnedCommand = null;
        while (dr.Read())
        {
            // read values off the data reader and create a new robotCommand
            int commandId = (int)dr["id"];
            string name = (string)dr["name"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            bool isMoveCommand = (bool)dr["ismovecommand"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            returnedCommand = new RobotCommand(commandId, name, isMoveCommand, createdDate, modifiedDate, description);
        }

        if (returnedCommand == null)
            throw new Exception("Command could not be updated.");

        return returnedCommand;
    }

    public bool DeleteRobotCommand(int id)
    {
        // check if command exists
        RobotCommand existingCommand = GetRobotCommandById(id); // this will throw error if it doesn't

        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand($"DELETE FROM robotcommand WHERE id=($1)", conn)
        {
            Parameters =
            {
                new() { Value = id }
            }
        };

        int affectedRows = cmd.ExecuteNonQuery();

        if (affectedRows == 1) return true;
        else return false;
    }
}