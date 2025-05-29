using Npgsql;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class UserADO : IUserDataAccess
{
    // Connection string is usually set in a config file for the ease of change.
    private const string CONNECTION_STRING = "Host=localhost;Username=cjhorn;Password=;Database=sit331";

    public List<UserModel> GetUsers()
    {
        var users = new List<UserModel>();
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT * FROM usermodel", conn);
        using var dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            // read values off the data reader and create a new user here and then add it to the result list.
            int id = (int)dr["id"];
            string email = (string)dr["email"];
            string firstName = (string)dr["firstname"];
            string lastName = (string)dr["lastname"];
            string passwordHash = (string)dr["passwordhash"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            string? role = dr["role"] == DBNull.Value ? null : (string)dr["role"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            UserModel user = new UserModel(id, email, firstName, lastName, passwordHash, createdDate, modifiedDate, description, role);
            users.Add(user);
        }
        return users;
    }

    public UserModel AddUser(UserModel newUser)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand("INSERT INTO usermodel (email, firstname, lastname, passwordhash, description, role, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5), ($6), ($7), ($8)) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = newUser.Email },
                new() { Value = newUser.FirstName },
                new() { Value = newUser.LastName },
                new() { Value = newUser.PasswordHash },
                new() { Value = newUser.Description == null ? DBNull.Value : newUser.Description },
                new() { Value = newUser.Role == null ? DBNull.Value : newUser.Role },
                new() { Value = newUser.CreatedDate },
                new() { Value = newUser.ModifiedDate }
            }
        };

        using var dr = cmd.ExecuteReader();

        UserModel? returnedUser = null;
        while (dr.Read())
        {
            // read values off the data reader and create a new user
            int id = (int)dr["id"];
            string email = (string)dr["email"];
            string firstName = (string)dr["firstname"];
            string lastName = (string)dr["lastname"];
            string passwordHash = (string)dr["passwordhash"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            string? role = dr["role"] == DBNull.Value ? null : (string)dr["role"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            returnedUser = new UserModel(id, email, firstName, lastName, passwordHash, createdDate, modifiedDate, description, role);
        }
        return returnedUser;
    }

    public UserModel UpdateUser(UserModel updatedUser)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand("UPDATE usermodel SET email=($1), firstname=($2), lastname=($3), passwordhash=($4), description=($5), role=($6), modifieddate=($7) WHERE id=($8) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = updatedUser.Email },
                new() { Value = updatedUser.FirstName },
                new() { Value = updatedUser.LastName },
                new() { Value = updatedUser.PasswordHash },
                new() { Value = updatedUser.Description == null ? DBNull.Value : updatedUser.Description },
                new() { Value = updatedUser.Role == null ? DBNull.Value : updatedUser.Role },
                new() { Value = updatedUser.ModifiedDate },
                new() { Value = updatedUser.Id }
            }
        };

        using var dr = cmd.ExecuteReader();

        UserModel? returnedUser = null;
        while (dr.Read())
        {
            // read values off the data reader and create a new user
            int id = (int)dr["id"];
            string email = (string)dr["email"];
            string firstName = (string)dr["firstname"];
            string lastName = (string)dr["lastname"];
            string passwordHash = (string)dr["passwordhash"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            string? role = dr["role"] == DBNull.Value ? null : (string)dr["role"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            returnedUser = new UserModel(id, email, firstName, lastName, passwordHash, createdDate, modifiedDate, description, role);
        }

        return returnedUser;
    }

    public bool DeleteUser(int id)
    {
        using var conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using var cmd = new NpgsqlCommand("DELETE FROM usermodel WHERE id=($1)", conn)
        {
            Parameters =
            {
                new() { Value = id }
            }
        };

        int affectedRows = cmd.ExecuteNonQuery();

        return affectedRows > 0;
    }
}
