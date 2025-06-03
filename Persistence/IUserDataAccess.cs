using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public interface IUserDataAccess
{
    UserAccount AddUser(UserAccount newUser);
    bool DeleteUser(int id);
    List<UserAccount> GetUsers();
    List<UserAccount> GetSpecificUsers(string role);
    UserAccount GetUserById(int id);
    UserAccount GetUserByEmail(string email);
    UserAccount UpdateUser(int id, UserUpdateDTO updatedUser);
    UserAccount UpdateUserCredentials(int id, UserLoginDTO credentials);
}