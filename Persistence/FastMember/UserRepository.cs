using Npgsql;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class UserRepository : IUserDataAccess, IRepository
{
    private IRepository _repo => this;

    public List<UserModel> GetUsers()
    {
        var users = _repo.ExecuteReader<UserModel>("SELECT * FROM public.usermodel");
        return users;
    }

    public UserModel AddUser(UserModel newUser)
    {
        var sqlCommand = "INSERT INTO usermodel (email, firstname, lastname, passwordhash, description, role, createddate, modifieddate) VALUES(($1), ($2), ($3), ($4), ($5), ($6), ($7), ($8)) RETURNING *;";
        var sqlParams = new NpgsqlParameter[]{
            new() { Value = newUser.Email },
            new() { Value = newUser.FirstName },
            new() { Value = newUser.LastName },
            new() { Value = newUser.PasswordHash },
            new() { Value = newUser.Description == null ? DBNull.Value : newUser.Description },
            new() { Value = newUser.Role == null ? DBNull.Value : newUser.Role },
            new() { Value = newUser.CreatedDate },
            new() { Value = newUser.ModifiedDate }
        };
        var result = _repo.ExecuteReader<UserModel>(sqlCommand, sqlParams).Single();
        return result;
    }

    public UserModel UpdateUser(UserModel updatedUser)
    {
        var sqlCommand = "UPDATE usermodel SET email=($1), firstname=($2), lastname=($3), passwordhash=($4), description=($5), role=($6), modifieddate=($7) WHERE id=($8) RETURNING *;";
        var sqlParams = new NpgsqlParameter[]{
            new() { Value = updatedUser.Email },
            new() { Value = updatedUser.FirstName },
            new() { Value = updatedUser.LastName },
            new() { Value = updatedUser.PasswordHash },
            new() { Value = updatedUser.Description == null ? DBNull.Value : updatedUser.Description },
            new() { Value = updatedUser.Role == null ? DBNull.Value : updatedUser.Role },
            new() { Value = updatedUser.ModifiedDate },
            new() { Value = updatedUser.Id },
        };
        var result = _repo.ExecuteReader<UserModel>(sqlCommand, sqlParams).Single();
        return result;
    }

    public bool DeleteUser(int id)
    {
        var sqlCommand = "DELETE FROM usermodel WHERE id=($1)";
        var sqlParams = new NpgsqlParameter[]{
            new() {Value = id}
        };
        var affectedRows = _repo.ExecuteNonQuery(sqlCommand, sqlParams);
        return affectedRows > 0;
    }
}