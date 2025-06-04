using Npgsql;
using robot_controller_api.Exceptions;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class RobotCommandADO : IRobotCommandDataAccess
{
    // Connection string is usually set in a config file for the ease of change.
    private const string CONNECTION_STRING = "Host=localhost; Username=cjhorn; Password=; Database=sit331";

    public List<RobotCommand> GetRobotCommands()
    {
        List<RobotCommand> robotCommands = new List<RobotCommand>();
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM robot_command", conn);
        using NpgsqlDataReader dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            // read values off the data reader and create a new robotCommand here and then add it to the result list.
            RobotCommand command = new RobotCommand
            {
                Id = (int)dr["id"],
                Name = (string)dr["name"],
                Description = dr["description"] == DBNull.Value ? null : (string)dr["description"],
                IsMoveCommand = (bool)dr["is_move_command"],
                CreatedDate = (DateTime)dr["created_date"],
                ModifiedDate = (DateTime)dr["modified_date"]
            };
            robotCommands.Add(command);
        }
        return robotCommands;
    }

    public List<RobotCommand> GetMoveRobotCommands()
    {
        List<RobotCommand> robotCommands = new List<RobotCommand>();
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM robot_command WHERE is_move_command = TRUE", conn);
        using NpgsqlDataReader dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            // read values off the data reader and create a new robotCommand here and then add it to the result list.
            RobotCommand command = new RobotCommand
            {
                Id = (int)dr["id"],
                Name = (string)dr["name"],
                Description = dr["description"] == DBNull.Value ? null : (string)dr["description"],
                IsMoveCommand = (bool)dr["is_move_command"],
                CreatedDate = (DateTime)dr["created_date"],
                ModifiedDate = (DateTime)dr["modified_date"]
            };
            robotCommands.Add(command);
        }
        return robotCommands;
    }

    public RobotCommand GetRobotCommandById(int id)
    {
        RobotCommand? command = null;
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT * FROM robot_command WHERE id = $1", conn)
        {
            Parameters = { new() { Value = id } }
        };
        using var dr = cmd.ExecuteReader();
        if (dr.Read())
        {
            // read values off the data reader and create a new robotCommand here and then return it.
            command = new RobotCommand
            {
                Id = (int)dr["id"],
                Name = (string)dr["name"],
                Description = dr["description"] == DBNull.Value ? null : (string)dr["description"],
                IsMoveCommand = (bool)dr["is_move_command"],
                CreatedDate = (DateTime)dr["created_date"],
                ModifiedDate = (DateTime)dr["modified_date"]
            };
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
        
        using var checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM robot_command WHERE name = $1", conn)
        {
            Parameters = { new() { Value = newCommand.Name } }
        };
        
        int nameCount = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (nameCount > 0)
            throw new DuplicateNameException(newCommand.Name);


        // insert new command
        using var cmd = new NpgsqlCommand("INSERT INTO robot_command (name, description, is_move_command, created_date, modified_date) VALUES(($1), ($2), ($3), ($4), ($5)) RETURNING *;", conn)
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
            returnedCommand = new RobotCommand
            {
                Id = (int)dr["id"],
                Name = (string)dr["name"],
                Description = dr["description"] == DBNull.Value ? null : (string)dr["description"],
                IsMoveCommand = (bool)dr["is_move_command"],
                CreatedDate = (DateTime)dr["created_date"],
                ModifiedDate = (DateTime)dr["modified_date"]
            };
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
        using var cmd = new NpgsqlCommand("UPDATE robot_command SET name=($1), description=($2), is_move_command=($3), modified_date=($4) WHERE id=($5) RETURNING *;", conn)
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
            returnedCommand = new RobotCommand
            {
                Id = (int)dr["id"],
                Name = (string)dr["name"],
                Description = dr["description"] == DBNull.Value ? null : (string)dr["description"],
                IsMoveCommand = (bool)dr["is_move_command"],
                CreatedDate = (DateTime)dr["created_date"],
                ModifiedDate = (DateTime)dr["modified_date"]
            };
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

        using var cmd = new NpgsqlCommand($"DELETE FROM robot_command WHERE id=($1)", conn)
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