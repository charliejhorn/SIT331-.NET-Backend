using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using robot_controller_api.Persistence;
using robot_controller_api.Authentication;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;

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
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not an admin</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpGet(), Authorize(Policy = "AdminOnly")]
    public IEnumerable<UserAccount> GetAllUsers()
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
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not an admin</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpGet("admin"), Authorize(Policy = "AdminOnly")]
    public IEnumerable<UserAccount> GetAdminUsersOnly()
    {
        return _usersRepo.GetSpecificUsers("Admin");
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
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not authorized to access this user</response>
    /// <response code="404">If a user with the provided ID can't be found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}", Name = "GetUser"), Authorize(Policy = "SelfOrAdmin")]
    public IActionResult GetUserById(int id)
    {
        try
        {
            UserAccount user = _usersRepo.GetUserById(id);
            return Ok(user);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
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
    public IActionResult AddUser(UserAccount newUser)
    {
        if(newUser == null) return BadRequest();

        try
        {
            UserAccount addedUser = _usersRepo.AddUser(newUser);
            return CreatedAtRoute("GetUser", new { id = addedUser.Id }, addedUser);
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            string fullError = ex.ToString(); // this includes inner exception details
            return BadRequest(new { message = ex.Message, details = fullError });
        }
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">The ID of the targeted user.</param>
    /// <param name="inputUser">An updated user from the HTTP request body.</param>
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
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not authorized to update this user</response>
    /// <response code="404">If a user with the provided ID can't be found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPut("{id}"), Authorize(Policy = "SelfOrAdmin")]
    public IActionResult UpdateUser(int id, UserUpdateDTO inputUser)
    {
        if (inputUser == null) return BadRequest("User data is required.");

        try
        {
            UserAccount updatedUser = _usersRepo.UpdateUser(id, inputUser);
            return Ok(updatedUser);
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            string fullError = ex.ToString(); // this includes inner exception details
            return BadRequest(new { message = ex.Message, details = fullError });
        }
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
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not authorized to delete this user</response>
    /// <response code="404">If a user with the provided ID can't be found</response>
    /// <response code="500">If an unexpected database error occurs during deletion.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}"), Authorize(Policy = "SelfOrAdmin")]
    public IActionResult DeleteUser(int id)
    {
        try
        {
            bool success = _usersRepo.DeleteUser(id);
            return NoContent(); // deletion was successful
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // database operation failed unexpectedly
            return StatusCode(500, new { message = "An error occurred while deleting the user.", details = ex.Message });
        }
        catch (Exception ex)
        {
            string fullError = ex.ToString(); // this includes inner exception details
            return BadRequest(new { message = ex.Message, details = fullError });
        }
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
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not authorized to update this user</response>
    /// <response code="404">If a user with the provided ID can't be found.</response>
    /// <response code="409">If a user with the same name as the new email provided already exists.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPatch("{id}"), Authorize(Policy = "SelfOrAdmin")]
    public IActionResult PatchUser(int id, UserLoginDTO? loginModel)
    {
        if (loginModel == null) return BadRequest("User data is required.");

        try
        {
            UserAccount updatedUser = _usersRepo.UpdateUserCredentials(id, loginModel);
            return Ok(updatedUser);
        }
        catch (DuplicateEmailException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes test users created for automated testing.
    /// </summary>
    /// <param name="cleanupRequest">JSON object containing array of email addresses to delete</param>
    /// <returns>Result of cleanup operation</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/users/cleanup-test-users
    ///     {
    ///         "emails": [
    ///             "test-admin@example.com",
    ///             "test-user@example.com"
    ///         ]
    ///     }
    ///
    /// This endpoint removes users with the provided email addresses.
    /// Only accessible by Admin users.
    /// </remarks>
    /// <response code="200">Test users cleaned up successfully</response>
    /// <response code="400">If the request body is null or invalid</response>
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="500">Internal server error during cleanup</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("cleanup-test-users"), Authorize(Policy = "AdminOnly")]
    public ActionResult CleanupTestUsers(CleanupTestAccountsDTO cleanupRequest)
    {
        if (cleanupRequest == null || cleanupRequest.AdminEmail == null || cleanupRequest.UserEmail == null)
        {
            return BadRequest(new { message = "Email addresses are required in the request body." });
        }

        try
        {
            UserAccount admin = _usersRepo.GetUserByEmail(cleanupRequest.AdminEmail);
            UserAccount user = _usersRepo.GetUserByEmail(cleanupRequest.UserEmail);

            bool adminSuccess = _usersRepo.DeleteUser(admin.Id);
            bool userSuccess = _usersRepo.DeleteUser(user.Id);

            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // database operation failed unexpectedly
            return StatusCode(500, new { message = "An error occurred while deleting the test accounts.", details = ex.Message });
        }
        catch (Exception ex)
        {
            string fullError = ex.ToString(); // this includes inner exception details
            return BadRequest(new { message = ex.Message, details = fullError });
        }
    }
}