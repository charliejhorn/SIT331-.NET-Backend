using Npgsql;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;
using robot_controller_api.Authentication;

namespace robot_controller_api.Persistence;

public class UserRepository : IUserDataAccess, IRepository
{
    private IRepository _repo => this;

    public List<UserAccount> GetUsers()
    {
        List<UserAccount> users = _repo.ExecuteReader<UserAccount>("SELECT * FROM user_account");
        return users;
    }

    public List<UserAccount> GetSpecificUsers(string role)
    {
        string sqlCommand = "SELECT * FROM user_account WHERE role=($1)";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = role }
        };
        List<UserAccount> users = _repo.ExecuteReader<UserAccount>(sqlCommand, sqlParams);
        return users;
    }

    public UserAccount GetUserById(int id)
    {
        string sqlCommand = "SELECT * FROM user_account WHERE id=($1)";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = id }
        };
        List<UserAccount> users = _repo.ExecuteReader<UserAccount>(sqlCommand, sqlParams);
        if (users.Count == 0)
        {
            throw new NotFoundException(id);
        }
        return users.First();
    }

    public UserAccount GetUserByEmail(string email)
    {
        string sqlCommand = "SELECT * FROM user_account WHERE email=($1)";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = email }
        };
        List<UserAccount> users = _repo.ExecuteReader<UserAccount>(sqlCommand, sqlParams);
        if (users.Count == 0)
        {
            throw new NotFoundException(email, "email");
        }
        return users.First();
    }

    public UserAccount AddUser(UserAccount newUser)
    {
        // check for duplicate email first
        string checkSqlCommand = "SELECT * FROM user_account WHERE email=($1)";
        NpgsqlParameter[] checkSqlParams = new NpgsqlParameter[]{
            new() { Value = newUser.Email }
        };
        List<UserAccount> existingUsers = _repo.ExecuteReader<UserAccount>(checkSqlCommand, checkSqlParams);
        if (existingUsers.Count > 0)
            throw new DuplicateEmailException(newUser.Email);

        // hash password
        newUser.PasswordHash = PasswordService.HashPassword(newUser.PasswordHash);

        newUser.CreatedDate = DateTime.Now;
        newUser.ModifiedDate = DateTime.Now;


        // send creation query
        string sqlCommand = "INSERT INTO user_account (email, first_name, last_name, password_hash, description, role, created_date, modified_date) VALUES(($1), ($2), ($3), ($4), ($5), ($6), ($7), ($8)) RETURNING *;";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = newUser.Email },
            new() { Value = newUser.FirstName },
            new() { Value = newUser.LastName },
            new() { Value = newUser.PasswordHash },
            new() { Value = newUser.Description == null ? DBNull.Value : newUser.Description },
            new() { Value = newUser.Role == null ? DBNull.Value : newUser.Role },
            new() { Value = newUser.CreatedDate },
            new() { Value = newUser.ModifiedDate }
        };
        UserAccount result = _repo.ExecuteReader<UserAccount>(sqlCommand, sqlParams).Single();
        return result;
    }

    public UserAccount UpdateUser(int id, UserUpdateDTO updatedUser)
    {
        // check if user exists first
        GetUserById(id); // this will throw NotFoundException if user doesn't exist

        string sqlCommand = "UPDATE user_account SET first_name=($1), last_name=($2), description=($3), role=($4), modified_date=($5) WHERE id=($6) RETURNING *;";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = updatedUser.FirstName },
            new() { Value = updatedUser.LastName },
            new() { Value = updatedUser.Description == null ? DBNull.Value : updatedUser.Description },
            new() { Value = updatedUser.Role == null ? DBNull.Value : updatedUser.Role },
            new() { Value = DateTime.Now },
            new() { Value = id },
        };
        UserAccount result = _repo.ExecuteReader<UserAccount>(sqlCommand, sqlParams).Single();
        return result;
    }

    public UserAccount UpdateUserCredentials(int id, UserLoginDTO credentials)
    {
        // check if user exists first
        GetUserById(id); // this will throw NotFoundException if user doesn't exist

        // check for duplicate email (unless the email belongs to the current user)
        string checkSqlCommand = "SELECT * FROM user_account WHERE email=($1) AND id!=($2)";
        NpgsqlParameter[] checkSqlParams = new NpgsqlParameter[]{
            new() { Value = credentials.Email },
            new() { Value = id }
        };
        List<UserAccount> existingUsers = _repo.ExecuteReader<UserAccount>(checkSqlCommand, checkSqlParams);
        if (existingUsers.Count > 0)
            throw new DuplicateEmailException(credentials.Email);

        // update email and password
        string sqlCommand = "UPDATE user_account SET email=($1), password_hash=($2), modified_date=($3) WHERE id=($4) RETURNING *;";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = credentials.Email },
            new() { Value = PasswordService.HashPassword(credentials.Password) },
            new() { Value = DateTime.Now },
            new() { Value = id },
        };
        UserAccount result = _repo.ExecuteReader<UserAccount>(sqlCommand, sqlParams).Single();
        return result;
    }

    public bool DeleteUser(int id)
    {
        // check if user exists first
        GetUserById(id); // this will throw NotFoundException if user doesn't exist

        string sqlCommand = "DELETE FROM user_account WHERE id=($1)";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() {Value = id}
        };
        int affectedRows = _repo.ExecuteNonQuery(sqlCommand, sqlParams);
        
        // ensure exactly one row was deleted
        if (affectedRows != 1)
        {
            throw new InvalidOperationException($"Expected to delete 1 row, but {affectedRows} rows were affected.");
        }
        
        return true;
    }
}