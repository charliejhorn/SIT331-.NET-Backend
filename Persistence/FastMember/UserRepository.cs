using Npgsql;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;
using robot_controller_api.Authentication;

namespace robot_controller_api.Persistence;

public class UserRepository : IUserDataAccess, IRepository
{
    private IRepository _repo => this;

    public List<UserModel> GetUsers()
    {
        List<UserModel> users = _repo.ExecuteReader<UserModel>("SELECT * FROM public.usermodel");
        return users;
    }

    public List<UserModel> GetSpecificUsers(string role)
    {
        string sqlCommand = "SELECT * FROM public.usermodel WHERE role=($1)";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = role }
        };
        List<UserModel> users = _repo.ExecuteReader<UserModel>(sqlCommand, sqlParams);
        return users;
    }

    public UserModel GetUserById(int id)
    {
        string sqlCommand = "SELECT * FROM public.usermodel WHERE id=($1)";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = id }
        };
        List<UserModel> users = _repo.ExecuteReader<UserModel>(sqlCommand, sqlParams);
        if (users.Count == 0)
        {
            throw new NotFoundException(id);
        }
        return users.First();
    }

    public UserModel GetUserByEmail(string email)
    {
        string sqlCommand = "SELECT * FROM public.usermodel WHERE email=($1)";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = email }
        };
        List<UserModel> users = _repo.ExecuteReader<UserModel>(sqlCommand, sqlParams);
        if (users.Count == 0)
        {
            throw new NotFoundException(email, "email");
        }
        return users.First();
    }

    public UserModel AddUser(UserModel newUser)
    {
        // check for duplicate email first
        string checkSqlCommand = "SELECT * FROM public.usermodel WHERE email=($1)";
        NpgsqlParameter[] checkSqlParams = new NpgsqlParameter[]{
            new() { Value = newUser.Email }
        };
        List<UserModel> existingUsers = _repo.ExecuteReader<UserModel>(checkSqlCommand, checkSqlParams);
        if (existingUsers.Count > 0)
            throw new DuplicateEmailException(newUser.Email);

        // hash password
        newUser.PasswordHash = PasswordService.HashPassword(newUser.PasswordHash);

        newUser.CreatedDate = DateTime.Now;
        newUser.ModifiedDate = DateTime.Now;


        // send creation query
        string sqlCommand = "INSERT INTO usermodel (email, firstname, lastname, passwordhash, description, role, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5), ($6), ($7), ($8)) RETURNING *;";
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
        UserModel result = _repo.ExecuteReader<UserModel>(sqlCommand, sqlParams).Single();
        return result;
    }

    public UserModel UpdateUser(int id, UserUpdateDTO updatedUser)
    {
        // check if user exists first
        GetUserById(id); // this will throw NotFoundException if user doesn't exist

        string sqlCommand = "UPDATE usermodel SET firstname=($1), lastname=($2), description=($3), role=($4), modifieddate=($5) WHERE id=($6) RETURNING *;";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = updatedUser.FirstName },
            new() { Value = updatedUser.LastName },
            new() { Value = updatedUser.Description == null ? DBNull.Value : updatedUser.Description },
            new() { Value = updatedUser.Role == null ? DBNull.Value : updatedUser.Role },
            new() { Value = DateTime.Now },
            new() { Value = id },
        };
        UserModel result = _repo.ExecuteReader<UserModel>(sqlCommand, sqlParams).Single();
        return result;
    }

    public UserModel UpdateUserCredentials(int id, UserLoginDTO credentials)
    {
        // check if user exists first
        GetUserById(id); // this will throw NotFoundException if user doesn't exist

        // check for duplicate email (unless the email belongs to the current user)
        string checkSqlCommand = "SELECT * FROM public.usermodel WHERE email=($1) AND id!=($2)";
        NpgsqlParameter[] checkSqlParams = new NpgsqlParameter[]{
            new() { Value = credentials.Email },
            new() { Value = id }
        };
        List<UserModel> existingUsers = _repo.ExecuteReader<UserModel>(checkSqlCommand, checkSqlParams);
        if (existingUsers.Count > 0)
            throw new DuplicateEmailException(credentials.Email);

        // update email and password
        string sqlCommand = "UPDATE usermodel SET email=($1), passwordhash=($2), modifieddate=($3) WHERE id=($4) RETURNING *;";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() { Value = credentials.Email },
            new() { Value = PasswordService.HashPassword(credentials.Password) },
            new() { Value = DateTime.Now },
            new() { Value = id },
        };
        UserModel result = _repo.ExecuteReader<UserModel>(sqlCommand, sqlParams).Single();
        return result;
    }

    public bool DeleteUser(int id)
    {
        // check if user exists first
        GetUserById(id); // this will throw NotFoundException if user doesn't exist

        string sqlCommand = "DELETE FROM usermodel WHERE id=($1)";
        NpgsqlParameter[] sqlParams = new NpgsqlParameter[]{
            new() {Value = id}
        };
        int affectedRows = _repo.ExecuteNonQuery(sqlCommand, sqlParams);
        return affectedRows > 0;
    }
}