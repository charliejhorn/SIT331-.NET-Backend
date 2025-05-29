using Microsoft.EntityFrameworkCore;
using robot_controller_api.Models;

namespace robot_controller_api.Persistence;

public class UserEF : IUserDataAccess
{
    private readonly RobotContext _context;

    public UserEF(RobotContext context)
    {
        _context = context;
    }

    public List<UserModel> GetUsers()
    {
        return _context.UserModels.ToList();
    }

    public UserModel AddUser(UserModel newUser)
    {
        _context.UserModels.Add(newUser);
        _context.SaveChanges();
        return newUser;
    }

    public UserModel UpdateUser(UserModel updatedUser)
    {
        _context.Entry(updatedUser).State = EntityState.Modified;
        _context.SaveChanges();
        return updatedUser;
    }

    public bool DeleteUser(int id)
    {
        var user = _context.UserModels.Find(id);
        if (user == null)
        {
            return false;
        }

        _context.UserModels.Remove(user);
        var affectedRows = _context.SaveChanges();
        return affectedRows > 0;
    }
}
