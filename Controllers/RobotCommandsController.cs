using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using robot_controller_api.Persistence;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;
using System.Text.Json;
using System.Text;

namespace robot_controller_api.Controllers;

[ApiController]
[Route("api/robot-commands")]
public class RobotCommandsController : ControllerBase
{
    private readonly IRobotCommandDataAccess _robotCommandsRepo;
    public RobotCommandsController(IRobotCommandDataAccess robotCommandsRepo)
    {
        _robotCommandsRepo = robotCommandsRepo;
    }

    /// <summary>
    /// Retrieves all robot commands.
    /// </summary>
    /// <returns>A list of robot commands</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/robot-commands
    ///
    /// </remarks>
    /// <response code="200">Returns a list of robot commands.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet(), AllowAnonymous]
    public IEnumerable<RobotCommand> GetAllRobotCommands()
    {
        return _robotCommandsRepo.GetRobotCommands();
    }

    /// <summary>
    /// Retrieves all robot commands that move the robot.
    /// </summary>
    /// <returns>A list of robot commands</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/robot-commands/move
    ///
    /// </remarks>
    /// <response code="200">Returns a list of robot commands</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("move"), AllowAnonymous]
    public IEnumerable<RobotCommand> GetMoveCommandsOnly()
    {
        List<RobotCommand> commands = _robotCommandsRepo.GetRobotCommands();
        return commands.Where((RobotCommand command) => command.IsMoveCommand);
    }

    /// <summary>
    /// Retrieves a specific robot command by ID.
    /// </summary>
    /// <param name="id">The id of the targeted robot command.</param>
    /// <returns>A singular robot command.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/robot-commands/1
    ///
    /// </remarks>
    /// <response code="200">Returns a robot command matching the provided ID.</response>
    /// <response code="404">If a robot command with the provided ID can't be found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}", Name = "GetRobotCommand"), AllowAnonymous]
    public IActionResult GetRobotCommandById(int id)
    {
        try
        {
            RobotCommand command = _robotCommandsRepo.GetRobotCommandById(id);
            return Ok(command);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new robot command.
    /// </summary>
    /// <param name="newCommand">A new robot command from the HTTP request body.</param>
    /// <returns>A newly created robot command.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/robot-commands
    ///     {
    ///        "name": "DANCE",
    ///        "isMoveCommand": true,
    ///        "description": "Salsa on the Moon"
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created robot command</response>
    /// <response code="400">If the provided robot command is null</response>
    /// <response code="409">If a robot command with the same name already exists.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost(), Authorize(Policy = "AdminOnly")]
    public IActionResult AddRobotCommand(RobotCommand newCommand)
    {
        if(newCommand == null) return BadRequest();

        try
        {
            RobotCommand addedCommand = _robotCommandsRepo.AddRobotCommand(newCommand);
            return CreatedAtRoute("GetRobotCommand", new { id = addedCommand.Id }, addedCommand);
        }
        catch (DuplicateNameException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing robot command.
    /// </summary>
    /// <param name="id">The id of the targeted robot command.</param>
    /// <param name="inputCommand">An updated robot command from the HTTP request body.</param>
    /// <returns>The updated robot command</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/robot-commands/1
    ///     {
    ///        "name": "Dance",
    ///        "isMoveCommand": true,
    ///        "description": "Salsa on the Moon"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns the updated robot command.</response>
    /// <response code="400">If the updated robot command provided is null, or if the robot command could not be updated.</response>
    /// <response code="404">If a robot command with the provided ID can't be found.</response>
    /// <response code="409">If a robot command with the same name as the new name provided already exists.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPut("{id}"), Authorize(Policy = "AdminOnly")]
    public IActionResult UpdateRobotCommand(int id, RobotCommand inputCommand)
    {
        try
        {
            RobotCommand? updatedCommand = _robotCommandsRepo.UpdateRobotCommand(id, inputCommand);
            return Ok(updatedCommand);
        }
        catch (DuplicateNameException ex)
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
    /// Deletes a specific robot command.
    /// </summary>
    /// <param name="id">The ID of the targeted robot command.</param>
    /// <returns>Nothing</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/robot-commands/1
    ///
    /// </remarks>
    /// <response code="204">If the robot command is deleted, returns no content.</response>
    /// <response code="404">If a robot command with the provided ID can't be found</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
    public IActionResult DeleteRobotCommand(int id)
    {
        try
        {
            bool success = _robotCommandsRepo.DeleteRobotCommand(id);

            if (success) return NoContent();
            else return NotFound();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Alters an existing robot command.
    /// </summary>
    /// <param name="id">The ID of the targeted robot command.</param>
    /// <returns>The updated robot command.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/robot-commands/1
    ///     {
    ///        "name": "DANCE",
    ///        "description": "Salsa on the Moon"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns the patched robot command.</response>
    /// <response code="400">If the new robot command data could not be parsed, or if the robot command could not be updated.</response>
    /// <response code="404">If a robot command with the provided ID can't be found.</response>
    /// <response code="409">If a robot command with the same name as the new name provided already exists.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPatch("{id}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> PatchRobotCommand(int id)
    {
        // fetch existing command
        List<RobotCommand> commands = _robotCommandsRepo.GetRobotCommands();
        RobotCommand? existingCommand = commands.Find(command => command.Id == id);
        if (existingCommand == null) return NotFound();

        // enable buffering so we can read the request body
        Request.EnableBuffering();
        Request.Body.Position = 0;
        
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();
        
        try 
        {
            // parse the JSON to see which fields were included
            using JsonDocument doc = JsonDocument.Parse(requestBody);
            JsonElement root = doc.RootElement;
            
            // for each field included, make change to existingCommand
            if (root.TryGetProperty("name", out JsonElement nameElement))
            {
                existingCommand.Name = nameElement.GetString();
            }
            
            if (root.TryGetProperty("description", out JsonElement descElement))
            {
                existingCommand.Description = descElement.ValueKind == JsonValueKind.Null ? 
                    null : descElement.GetString();
            }
            
            if (root.TryGetProperty("ismovecommand", out JsonElement moveElement))
            {
                existingCommand.IsMoveCommand = moveElement.GetBoolean();
            }
            
            // check name doesn't exist
            if(commands.Exists(command => command.Id != id && command.Name == existingCommand.Name)) return Conflict("Name already exists.");

            existingCommand.ModifiedDate = DateTime.UtcNow;
            
            // push change to database
            RobotCommand? returnedCommand = _robotCommandsRepo.UpdateRobotCommand(existingCommand.Id, existingCommand);

            if (returnedCommand == null) return BadRequest("RobotCommand could not be patched.");

            return Ok(returnedCommand);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            return BadRequest("Invalid JSON format");
        }
    }
}

