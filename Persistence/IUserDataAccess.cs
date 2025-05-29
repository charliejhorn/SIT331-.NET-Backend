namespace robot_controller_api.Persistence;

public interface IUserDataAccess
{
    UserModel AddUser(UserModel newUser);
    bool DeleteUser(int id);
    List<UserModel> GetUsers();
    UserModel UpdateUser(UserModel updatedUser);
}