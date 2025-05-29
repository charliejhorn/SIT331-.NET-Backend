using Microsoft.AspNetCore.Mvc;
using robot_controller_api.Persistence;
using Microsoft.AspNetCore.Authorization;
using robot_controller_api.Authentication;

namespace robot_controller_api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserDataAccess _usersRepo;

    public UsersController(IUserDataAccess usersRepo)
    {
        _usersRepo = usersRepo;
    }

    // ENDPOINTS

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <returns>A list of users</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/users
    ///
    /// </remarks>
    /// <response code="200">Returns a list of users.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet(), Authorize(Policy = "AdminOnly")]
    public IEnumerable<UserModel> GetAllUsers()
    {
        return _usersRepo.GetUsers();
    }

    /// <summary>
    /// Retrieves all users that have the role 'admin'.
    /// </summary>
    /// <returns>A list of users</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/users/admin
    ///
    /// </remarks>
    /// <response code="200">Returns a list of users</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("admin"), Authorize(Policy = "AdminOnly")]
    public IEnumerable<UserModel> GetAdminUsersOnly()
    {
        List<UserModel> users = _usersRepo.GetUsers();
        return users.Where((UserModel user) => user.Role == "admin");
    }

    /// <summary>
    /// Retrieves a specific user by ID.
    /// </summary>
    /// <param name="id">The id of the targeted user.</param>
    /// <returns>A singular user.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/users/1
    ///
    /// </remarks>
    /// <response code="200">Returns a user matching the provided ID.</response>
    /// <response code="404">If a user with the provided ID can't be found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}", Name = "GetUser"), Authorize(Policy = "SelfOrAdmin")]
    public IActionResult GetUserById(int id)
    {
        List<UserModel> users = _usersRepo.GetUsers();

        UserModel? user = users.Find((UserModel user) => user.Id == id);

        if(user == null) return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="newUser">A new user from the HTTP request body.</param>
    /// <returns>A newly created user.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/users
    ///     {
    ///         "email": "swifty@gmail.com",
    ///         "firstname": "Taylor",
    ///         "lastname": "Swift",
    ///         "passwordhash": "xxxx",
    ///         "description": "Professional artist.",
    ///         "role": "admin"
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created user.</response>
    /// <response code="400">If the provided user is null.</response>
    /// <response code="409">If a user with the same email already exists.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost(), AllowAnonymous]
    public IActionResult AddUser(UserModel newUser)
    {
        if(newUser == null) return BadRequest();

        List<UserModel> users = _usersRepo.GetUsers();
        if(users.Exists(user => user.Email == newUser.Email)) return Conflict("Email already exists.");

        newUser.PasswordHash = PasswordService.HashPassword(newUser.PasswordHash);
        newUser.CreatedDate = DateTime.Now;
        newUser.ModifiedDate = DateTime.Now;

        UserModel? addedUser = _usersRepo.AddUser(newUser);

        if (addedUser == null) return BadRequest("User could not be created.");

        return CreatedAtRoute("GetUser", new { id = addedUser.Id }, addedUser);
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">The ID of the targeted user.</param>
    /// <param name="updatedUserFromRequest">An updated user from the HTTP request body.</param>
    /// <returns>The updated user.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/users/1
    ///     {
    ///         "firstname": "Taylor",
    ///         "lastname": "Swift",
    ///         "description": "World renowned singer.",
    ///         "role": "User"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns the updated user.</response>
    /// <response code="400">If the updated user provided is null, or if the user could not be updated.</response>
    /// <response code="404">If a user with the provided ID can't be found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("{id}"), Authorize(Policy = "SelfOrAdmin")]
    public IActionResult UpdateUser(int id, UserUpdateModel updatedUserFromRequest)
    {
        if (updatedUserFromRequest == null) return BadRequest("User data is required.");

        // check user exists
        List<UserModel> users = _usersRepo.GetUsers();
        UserModel? existingUser = users.Find(user => user.Id == id);
        if (existingUser == null) return NotFound();

        UserModel updatedUser = new UserModel();

        updatedUser.Id = existingUser.Id; // match correct ID
        updatedUser.FirstName = updatedUserFromRequest.FirstName; // use new first name
        updatedUser.LastName = updatedUserFromRequest.LastName; // use new last name
        updatedUser.Description = updatedUserFromRequest.Description; // use new description
        updatedUser.Role = updatedUserFromRequest.Role;
        updatedUser.Email = existingUser.Email; // keep original email
        updatedUser.PasswordHash = existingUser.PasswordHash; // keep original password
        updatedUser.CreatedDate = existingUser.CreatedDate; // keep original creation date
        updatedUser.ModifiedDate = DateTime.UtcNow;

        // push change to database
        UserModel? returnedUser = _usersRepo.UpdateUser(updatedUser);

        if (returnedUser == null) return BadRequest("User could not be updated.");

        return Ok(returnedUser);
    }

    /// <summary>
    /// Deletes a specific user.
    /// </summary>
    /// <param name="id">The ID of the targeted user.</param>
    /// <returns>Nothing</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/users/1
    ///
    /// </remarks>
    /// <response code="204">If the user is deleted, returns no content.</response>
    /// <response code="404">If a user with the provided ID can't be found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id}"), Authorize(Policy = "SelfOrAdmin")]
    public IActionResult DeleteUser(int id)
    {
        bool success = _usersRepo.DeleteUser(id);

        if (success) return NoContent();
        else return NotFound();
    }

    /// <summary>
    /// Alters the email or password of an existing user.
    /// </summary>
    /// <param name="id">The ID of the targeted user.</param>
    /// <param name="loginModel">An updated email and password from the request body.</param>
    /// <returns>The updated user.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/users/1
    ///     {
    ///        "email": "taylorswift@gmail.com",
    ///        "passwordhash": "yyyy"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns the patched user.</response>
    /// <response code="400">If the new email and password was not provided, or if the user could not be updated.</response>
    /// <response code="404">If a user with the provided ID can't be found.</response>
    /// <response code="409">If a user with the same name as the new email provided already exists.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPatch("{id}"), Authorize(Policy = "SelfOrAdmin")]
    public IActionResult PatchUser(int id, LoginModel? loginModel)
    {
        if (loginModel == null) return BadRequest("User data is required.");

        // check user exists
        List<UserModel> users = _usersRepo.GetUsers();
        UserModel? existingUser = users.Find(user => user.Id == id);
        if (existingUser == null) return NotFound();
            
        // check email doesn't already exist
        if(users.Exists(user => user.Id != id && user.Email == loginModel.Email)) return Conflict("Email already exists.");

        existingUser.PasswordHash = PasswordService.HashPassword(loginModel.Password); // set new password
        existingUser.Email = loginModel.Email; // set new email
        existingUser.ModifiedDate = DateTime.UtcNow; // updated modified date
        
        // push change to database
        UserModel? returnedUser = _usersRepo.UpdateUser(existingUser);

        if (returnedUser == null) return BadRequest("User could not be patched.");

        return Ok(returnedUser);
    }
}