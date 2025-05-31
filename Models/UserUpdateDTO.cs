namespace robot_controller_api.Models;

public class UserUpdateDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Description { get; set; }
    public string Role { get; set; }

    public UserUpdateDTO(string firstName, string lastName, string description, string role)
    {
        FirstName = firstName;
        LastName = lastName;
        Description = description;
        Role = role;
    }
}