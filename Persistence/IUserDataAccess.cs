using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public interface IUserDataAccess
{
    UserModel AddUser(UserModel newUser);
    bool DeleteUser(int id);
    List<UserModel> GetUsers();
    List<UserModel> GetSpecificUsers(string role);
    UserModel GetUserById(int id);
    UserModel GetUserByEmail(string email);
    UserModel UpdateUser(int id, UserUpdateDTO updatedUser);
    UserModel UpdateUserCredentials(int id, UserLoginDTO credentials);
}