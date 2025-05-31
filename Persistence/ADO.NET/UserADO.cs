using Npgsql;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;
using robot_controller_api.Authentication;

namespace robot_controller_api.Persistence;

public class UserADO : IUserDataAccess
{
    // Connection string is usually set in a config file for the ease of change.
    private const string CONNECTION_STRING = "Host=localhost;Username=cjhorn;Password=;Database=sit331";

    public List<UserModel> GetUsers()
    {
        List<UserModel> users = new List<UserModel>();
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM usermodel", conn);
        using NpgsqlDataReader dr = cmd.ExecuteReader();
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
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        
        // check for duplicate email first
        using NpgsqlCommand checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM usermodel WHERE email = $1", conn)
        {
            Parameters = { new() { Value = newUser.Email } }
        };
        
        int nameExists = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (nameExists > 0)
            throw new DuplicateEmailException(newUser.Email);

        // hash password
        newUser.PasswordHash = PasswordService.HashPassword(newUser.PasswordHash);

        newUser.CreatedDate = DateTime.Now;
        newUser.ModifiedDate = DateTime.Now;

        // insert new user
        using NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO usermodel (email, firstname, lastname, passwordhash, description, role, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5), ($6), ($7), ($8)) RETURNING *;", conn)
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


        // read new user that was returned from query
        using NpgsqlDataReader dr = cmd.ExecuteReader();

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
         
        if (returnedUser == null)
        {
            throw new InvalidOperationException("Failed to add user to database.");
        }
        
        return returnedUser;
    }

    public List<UserModel> GetSpecificUsers(string role)
    {
        List<UserModel> users = new List<UserModel>();
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM usermodel WHERE role=($1)", conn)
        {
            Parameters =
            {
                new() { Value = role }
            }
        };
        using NpgsqlDataReader dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            // read values off the data reader and create a new user here and then add it to the result list.
            int id = (int)dr["id"];
            string email = (string)dr["email"];
            string firstName = (string)dr["firstname"];
            string lastName = (string)dr["lastname"];
            string passwordHash = (string)dr["passwordhash"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            string? userRole = dr["role"] == DBNull.Value ? null : (string)dr["role"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            UserModel user = new UserModel(id, email, firstName, lastName, passwordHash, createdDate, modifiedDate, description, userRole);
            users.Add(user);
        }
        return users;
    }

    public UserModel GetUserById(int id)
    {
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM usermodel WHERE id=($1)", conn)
        {
            Parameters =
            {
                new() { Value = id }
            }
        };
        using NpgsqlDataReader dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            // read values off the data reader and create a new user
            string email = (string)dr["email"];
            string firstName = (string)dr["firstname"];
            string lastName = (string)dr["lastname"];
            string passwordHash = (string)dr["passwordhash"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            string? role = dr["role"] == DBNull.Value ? null : (string)dr["role"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            return new UserModel(id, email, firstName, lastName, passwordHash, createdDate, modifiedDate, description, role);
        }

        // if nothing was returned in the DataReader, then by default throw NotFound
        throw new NotFoundException(id);
    }

    public UserModel GetUserByEmail(string email)
    {
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();
        using NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM usermodel WHERE email=($1)", conn)
        {
            Parameters =
            {
                new() { Value = email }
            }
        };
        using NpgsqlDataReader dr = cmd.ExecuteReader();
        while (dr.Read())
        {
            // read values off the data reader and create a new user
            int id = (int)dr["id"];
            string firstName = (string)dr["firstname"];
            string lastName = (string)dr["lastname"];
            string passwordHash = (string)dr["passwordhash"];
            string? description = dr["description"] == DBNull.Value ? null : (string)dr["description"];
            string? role = dr["role"] == DBNull.Value ? null : (string)dr["role"];
            DateTime createdDate = (DateTime)dr["createddate"];
            DateTime modifiedDate = (DateTime)dr["modifieddate"];

            return new UserModel(id, email, firstName, lastName, passwordHash, createdDate, modifiedDate, description, role);
        }

        // if nothing was returned in the DataReader, then by default throw NotFound
        throw new NotFoundException(email, "email");
    }

    public UserModel UpdateUser(int id, UserUpdateDTO updatedUser)
    {
        // check if user exists first
        GetUserById(id); // this will throw NotFoundException if user doesn't exist


        // send update query
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using NpgsqlCommand cmd = new NpgsqlCommand("UPDATE usermodel SET firstname=($1), lastname=($2), description=($3), role=($4), modifieddate=($5) WHERE id=($6) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = updatedUser.FirstName },
                new() { Value = updatedUser.LastName },
                new() { Value = updatedUser.Description == null ? DBNull.Value : updatedUser.Description },
                new() { Value = updatedUser.Role == null ? DBNull.Value : updatedUser.Role },
                new() { Value = DateTime.Now },
                new() { Value = id }
            }
        };


        // read returned user from query
        using NpgsqlDataReader dr = cmd.ExecuteReader();

        UserModel? returnedUser = null;
        while (dr.Read())
        {
            // read values off the data reader and create a new user
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

        if (returnedUser == null)
        {
            throw new Exception("User could not be updated.");
        }

        return returnedUser;
    }

    public UserModel UpdateUserCredentials(int id, UserLoginDTO credentials)
    {
        // check if user exists first
        GetUserById(id); // this will throw NotFoundException if user doesn't exist

        // check for duplicate email (unless the email belongs to the current user)
        using NpgsqlConnection checkConn = new NpgsqlConnection(CONNECTION_STRING);
        checkConn.Open();
        using NpgsqlCommand checkCmd = new NpgsqlCommand("SELECT COUNT(*) FROM usermodel WHERE email = $1 AND id != $2", checkConn)
        {
            Parameters = 
            { 
                new() { Value = credentials.Email },
                new() { Value = id }
            }
        };
        
        int duplicateCount = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (duplicateCount > 0)
            throw new DuplicateEmailException(credentials.Email);

        // send update query
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using NpgsqlCommand cmd = new NpgsqlCommand("UPDATE usermodel SET email=($1), passwordhash=($2), modifieddate=($3) WHERE id=($4) RETURNING *;", conn)
        {
            Parameters =
            {
                new() { Value = credentials.Email },
                new() { Value = PasswordService.HashPassword(credentials.Password) },
                new() { Value = DateTime.Now },
                new() { Value = id }
            }
        };

        // read returned user from query
        using NpgsqlDataReader dr = cmd.ExecuteReader();

        UserModel? returnedUser = null;
        while (dr.Read())
        {
            // read values off the data reader and create a new user
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

        if (returnedUser == null)
        {
            throw new Exception("User credentials could not be updated.");
        }

        return returnedUser;
    }

    public bool DeleteUser(int id)
    {
        // check if user exists first
        GetUserById(id); // this will throw NotFoundException if user doesn't exist

        // send delete query
        using NpgsqlConnection conn = new NpgsqlConnection(CONNECTION_STRING);
        conn.Open();

        using NpgsqlCommand cmd = new NpgsqlCommand("DELETE FROM usermodel WHERE id=($1)", conn)
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
