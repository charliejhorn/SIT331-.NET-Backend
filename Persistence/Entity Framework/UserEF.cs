using Microsoft.EntityFrameworkCore;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;
using robot_controller_api.Authentication;
using robot_controller_api.Contexts;

namespace robot_controller_api.Persistence;

public class UserEF : IUserDataAccess
{
    private readonly RobotContext _context;

    public UserEF(RobotContext context)
    {
        _context = context;
    }

    public List<UserAccount> GetUsers()
    {
        return _context.UserAccounts.ToList();
    }

    public List<UserAccount> GetSpecificUsers(string role)
    {
        return _context.UserAccounts.Where(u => u.Role == role).ToList();
    }

    public UserAccount GetUserById(int id)
    {
        UserAccount? user = _context.UserAccounts.Find(id);
        if (user == null)
        {
            throw new NotFoundException(id);
        }
        return user;
    }

    public UserAccount GetUserByEmail(string email)
    {
        UserAccount? user = _context.UserAccounts.FirstOrDefault(u => u.Email == email);
        if (user == null)
        {
            throw new NotFoundException(email, "email");
        }
        return user;
    }

    public UserAccount AddUser(UserAccount newUser)
    {
        // check for duplicate email first
        bool duplicateEmailExists = _context.UserAccounts.Any(user => user.Email == newUser.Email);
        if (duplicateEmailExists)
            throw new DuplicateEmailException(newUser.Email);

        // hash password
        newUser.PasswordHash = PasswordService.HashPassword(newUser.PasswordHash);

        // Use DateTime.UtcNow to avoid timezone serialization issues
        var utcNow = DateTime.UtcNow;
        newUser.CreatedDate = utcNow;
        newUser.ModifiedDate = utcNow;

        _context.UserAccounts.Add(newUser);
        _context.SaveChanges();
        return newUser;
    }

    public UserAccount UpdateUser(int id, UserUpdateDTO updatedUser)
    {
        UserAccount? existingUser = _context.UserAccounts.Find(id);
        if (existingUser == null)
        {
            throw new NotFoundException(id);
        }

        existingUser.FirstName = updatedUser.FirstName;
        existingUser.LastName = updatedUser.LastName;
        existingUser.Description = updatedUser.Description;
        existingUser.Role = updatedUser.Role;
        existingUser.ModifiedDate = DateTime.UtcNow;

        _context.SaveChanges();
        return existingUser;
    }

    public UserAccount UpdateUserCredentials(int id, UserLoginDTO credentials)
    {
        // check if user exists first
        UserAccount? existingUser = _context.UserAccounts.Find(id);
        if (existingUser == null)
        {
            throw new NotFoundException(id);
        }

        // check for duplicate email (unless the email belongs to the current user)
        bool duplicateEmailExists = _context.UserAccounts.Any(user => user.Id != id && user.Email == credentials.Email);
        if (duplicateEmailExists)
            throw new DuplicateEmailException(credentials.Email);

        // update email and password
        existingUser.Email = credentials.Email;
        existingUser.PasswordHash = PasswordService.HashPassword(credentials.Password);
        existingUser.ModifiedDate = DateTime.UtcNow;

        _context.SaveChanges();
        return existingUser;
    }

    public bool DeleteUser(int id)
    {
        // check if user exists first
        UserAccount existingUser = GetUserById(id); // this will throw if not found

        _context.UserAccounts.Remove(existingUser);
        int affectedRows = _context.SaveChanges();
        
        // ensure exactly one row was deleted
        if (affectedRows != 1)
        {
            throw new InvalidOperationException($"Expected to delete 1 row, but {affectedRows} rows were affected.");
        }
        
        return true;
    }
}
