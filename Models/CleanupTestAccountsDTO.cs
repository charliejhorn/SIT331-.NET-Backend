namespace robot_controller_api.Models;

public class CleanupTestAccountsDTO
{
    public string AdminEmail { get; set; }
    public string UserEmail { get; set; }

    public CleanupTestAccountsDTO(string adminEmail, string userEmail)
    {
        AdminEmail = adminEmail;
        UserEmail = userEmail;
    }
}
