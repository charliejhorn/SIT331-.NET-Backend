using Microsoft.EntityFrameworkCore;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;
using robot_controller_api.Authentication;

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

    public List<UserModel> GetSpecificUsers(string role)
    {
        return _context.UserModels.Where(u => u.Role == role).ToList();
    }

    public UserModel GetUserById(int id)
    {
        UserModel? user = _context.UserModels.Find(id);
        if (user == null)
        {
            throw new NotFoundException(id);
        }
        return user;
    }

    public UserModel GetUserByEmail(string email)
    {
        UserModel? user = _context.UserModels.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            throw new NotFoundException(email, "email");
        }
        return user;
    }

    public UserModel AddUser(UserModel newUser)
    {
        // check for duplicate email first
        bool duplicateEmailExists = _context.UserModels.Any(user => user.Email == newUser.Email);
        if (duplicateEmailExists)
            throw new DuplicateEmailException(newUser.Email);

        // hash password
        newUser.PasswordHash = PasswordService.HashPassword(newUser.PasswordHash);

        newUser.CreatedDate = DateTime.Now;
        newUser.ModifiedDate = DateTime.Now;

        _context.UserModels.Add(newUser);
        _context.SaveChanges();
        return newUser;
    }

    public UserModel UpdateUser(int id, UserUpdateDTO updatedUser)
    {
        UserModel? existingUser = _context.UserModels.Find(id);
        if (existingUser == null)
        {
            throw new NotFoundException(id);
        }

        existingUser.FirstName = updatedUser.FirstName;
        existingUser.LastName = updatedUser.LastName;
        existingUser.Description = updatedUser.Description;
        existingUser.Role = updatedUser.Role;
        existingUser.ModifiedDate = DateTime.Now;

        _context.SaveChanges();
        return existingUser;
    }

    public UserModel UpdateUserCredentials(int id, UserLoginDTO credentials)
    {
        // check if user exists first
        UserModel? existingUser = _context.UserModels.Find(id);
        if (existingUser == null)
        {
            throw new NotFoundException(id);
        }

        // check for duplicate email (unless the email belongs to the current user)
        bool duplicateEmailExists = _context.UserModels.Any(user => user.Id != id && user.Email == credentials.Email);
        if (duplicateEmailExists)
            throw new DuplicateEmailException(credentials.Email);

        // update email and password
        existingUser.Email = credentials.Email;
        existingUser.PasswordHash = PasswordService.HashPassword(credentials.Password);
        existingUser.ModifiedDate = DateTime.Now;

        _context.SaveChanges();
        return existingUser;
    }

    public bool DeleteUser(int id)
    {
        UserModel? user = _context.UserModels.Find(id);
        if (user == null)
        {
            return false;
        }

        _context.UserModels.Remove(user);
        int affectedRows = _context.SaveChanges();
        return affectedRows > 0;
    }
}
