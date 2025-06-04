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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet(), Authorize(Policy = "AdminOnly"), AllowAnonymous]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    /// <response code="404">If a user with the provided ID can't be found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    /// <response code="404">If a user with the provided ID can't be found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    /// <response code="404">If a user with the provided ID can't be found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id}"), Authorize(Policy = "SelfOrAdmin")]
    public IActionResult DeleteUser(int id)
    {
        try
        {
            bool success = _usersRepo.DeleteUser(id);

            if (success) return NoContent();
            else return NotFound();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
    /// <response code="404">If a user with the provided ID can't be found.</response>
    /// <response code="409">If a user with the same name as the new email provided already exists.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
}