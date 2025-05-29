using Npgsql;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class RobotCommandADO : IRobotCommandDataAccess
{
    // Connection string is usually set in a config file for the ease of change.
    private const string CONNECTION_STRING = "Host=localhost;Username=cjhorn;Password=;Database=sit331";

    public List<RobotCommand> GetRobotCommands()
    {
        var robotCommands = new List<RobotCommand>();
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

    public RobotCommand? AddRobotCommand(RobotCommand newCommand)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand("INSERT INTO robotcommand (name, description, ismovecommand, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5)) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = newCommand.Name },
                new() { Value = newCommand.Description == null ? DBNull.Value : newCommand.Description },
                new() { Value = newCommand.IsMoveCommand },
                new() { Value = newCommand.CreatedDate },
                new() { Value = newCommand.ModifiedDate }
            }
        };

        using var dr = cmd.ExecuteReader();

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
        return returnedCommand;
    }

    public RobotCommand? UpdateRobotCommand(RobotCommand updatedCommand)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand("UPDATE robotcommand SET name=($1), description=($2), ismovecommand=($3), modifieddate=($4) WHERE id=($5) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = updatedCommand.Name },
                new() { Value = updatedCommand.Description == null ? DBNull.Value : updatedCommand.Description },
                new() { Value = updatedCommand.IsMoveCommand },
                new() { Value = updatedCommand.ModifiedDate },
                new() { Value = updatedCommand.Id }
            }
        };

        using var dr = cmd.ExecuteReader();

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

        return returnedCommand;
    }

    public bool DeleteRobotCommand(int id)
    {
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