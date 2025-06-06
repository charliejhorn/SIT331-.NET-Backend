using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using robot_controller_api.Persistence;
using robot_controller_api.Models;
using robot_controller_api.Exceptions;
using System.Text.Json;
using System.Text;

namespace robot_controller_api.Controllers;

[ApiController]
[Route("api/maps")]
public class MapsController : ControllerBase
{
    private readonly IMapDataAccess _mapsRepo;
    public MapsController(IMapDataAccess mapsRepo)
    {
        _mapsRepo = mapsRepo;
    }

    
    /// <summary>
    /// Retrieves all maps.
    /// </summary>
    /// <returns>A list of maps</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/maps
    ///
    /// </remarks>
    /// <response code="200">Returns a list of maps.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet(), AllowAnonymous]
    public IEnumerable<Map> GetAllMaps()
    {
        return _mapsRepo.GetMaps();
    }


    /// <summary>
    /// Retrieves all maps that are square.
    /// </summary>
    /// <returns>A list of maps</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/maps/square
    ///
    /// </remarks>
    /// <response code="200">Returns a list of maps</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("square"), AllowAnonymous]
    public IEnumerable<Map> GetAllSquareMaps()
    {
        return _mapsRepo.GetSquareMaps();
    }    /// <summary>
    /// Retrieves a specific map by ID.
    /// </summary>
    /// <param name="id">The ID of the targeted map.</param>
    /// <returns>A singular map.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/maps/1
    ///
    /// </remarks>
    /// <response code="200">Returns a map matching the provided ID.</response>
    /// <response code="404">If a map with the provided ID can't be found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}", Name = "GetMap"), AllowAnonymous]
    public IActionResult GetMapById(int id)
    {
        try
        {
            Map map = _mapsRepo.GetMapById(id);
            return Ok(map);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }    /// <summary>
    /// Creates a new map.
    /// </summary>
    /// <param name="newMap">A new map from the HTTP request body.</param>
    /// <returns>A newly created map.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/maps
    ///     {
    ///        "name": "Moon",
    ///        "description": "Made of cheese!",
    ///        "columns": 50,
    ///        "rows": 64
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created map</response>
    /// <response code="400">If the provided map is null</response>
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="409">If a map with the same name already exists.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost(), Authorize(Policy = "AdminOnly")]
    public IActionResult AddMap(Map newMap)
    {
        if(newMap == null) return BadRequest();

        try
        {
            Map addedMap = _mapsRepo.AddMap(newMap);
            return CreatedAtRoute("GetMap", new { id = addedMap.Id }, addedMap);
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
    /// Updates an existing map.
    /// </summary>
    /// <param name="id">The id of the targeted map.</param>
    /// <param name="updatedMap">An updated map from the HTTP request body.</param>
    /// <returns>The updated map.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/maps
    ///     {
    ///        "name": "Moon",
    ///        "description": "Made of cheese!",
    ///        "columns": 50,
    ///        "rows": 64
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns the updated map.</response>
    /// <response code="400">If the updated map provided is null, or if the map could not be updated.</response>
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If a map with the provided id can't be found.</response>
    /// <response code="409">If a map with the same name as the new name provided already exists.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPut("{id}"), Authorize(Policy = "AdminOnly")]
    public IActionResult UpdateMap(int id, Map updatedMap)
    {
        try
        {
            Map returnedMap = _mapsRepo.UpdateMap(id, updatedMap);
            return Ok(returnedMap);
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
    /// Deletes a specific map.
    /// </summary>
    /// <param name="id">The id of the targeted map.</param>
    /// <returns>Nothing</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/maps/1
    ///
    /// </remarks>
    /// <response code="204">If the map is deleted, returns no content.</response>
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If a map with the provided ID can't be found.</response>
    /// <response code="500">If an unexpected database error occurs during deletion.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
    public IActionResult DeleteMap(int id)
    {
        try
        {
            bool success = _mapsRepo.DeleteMap(id);
            return NoContent(); // deletion was successful
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            // database operation failed unexpectedly
            return StatusCode(500, new { message = "An error occurred while deleting the map.", details = ex.Message });
        }
    }


    /// <summary>
    /// Test if a coordinate exists on a map.
    /// </summary>
    /// <param name="id">The ID of the targeted map.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns>A boolean.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/maps/1/10-12
    ///
    /// </remarks>
    /// <response code="200">Returns a boolean identifying if the coordinate exists on the provided map.</response>
    /// <response code="401">If no authentication is provided</response>
    /// <response code="404">If a map with the provided ID can't be found.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}/{x}-{y}"), Authorize(Policy = "UserOnly")]
    public IActionResult GetCoordinate(int id, int x, int y)
    {
        try
        {
            Map targetMap = _mapsRepo.GetMapById(id);
            bool xValid = 0 <= x && x < targetMap.Columns;
            bool yValid = 0 <= y && y < targetMap.Rows;
            bool isOnMap = xValid && yValid;
            return Ok(isOnMap);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }    /// <summary>
    /// Alters an existing map.
    /// </summary>
    /// <param name="id">The id of the targeted map.</param>
    /// <returns>The updated map.</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/maps/1
    ///     {
    ///        "name": "MARS",
    ///        "rows": 35
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Returns the patched map.</response>
    /// <response code="400">If the new map data could not be parsed, or if the map could not be updated.</response>
    /// <response code="401">If no authentication is provided</response>
    /// <response code="403">If the user is not an admin</response>
    /// <response code="404">If a map with the provided ID can't be found.</response>
    /// <response code="409">If a map with the same name as the new name provided already exists.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPatch("{id}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> PatchMap(int id)
    {
        // enable buffering so we can read the request body
        Request.EnableBuffering();
        Request.Body.Position = 0;
        
        using StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8);
        string requestBody = await reader.ReadToEndAsync();

        try
        {
            Map target = _mapsRepo.GetMapById(id);

            // parse the JSON to see which fields were included
            using JsonDocument doc = JsonDocument.Parse(requestBody);
            JsonElement root = doc.RootElement;

            // for each field included, make change to target
            if (root.TryGetProperty("columns", out JsonElement columnsElement))
            {
                target.Columns = columnsElement.GetInt32();
            }

            if (root.TryGetProperty("rows", out JsonElement rowsElement))
            {
                target.Rows = rowsElement.GetInt32();
            }
            
            if (root.TryGetProperty("name", out JsonElement nameElement))
            {
                target.Name = nameElement.GetString()!;
            }
            
            if (root.TryGetProperty("description", out JsonElement descElement))
            {
                target.Description = descElement.ValueKind == JsonValueKind.Null ? 
                    null : descElement.GetString();
            }

            // push change to database
            Map returnedMap = _mapsRepo.UpdateMap(id, target);

            return Ok(returnedMap);
        }
        catch (DuplicateNameException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            return BadRequest(new { message = "Invalid JSON format" });
        }
        catch (Exception ex)
        {
            string fullError = ex.ToString(); // this includes inner exception details
            return BadRequest(new { message = ex.Message, details = fullError });
        }
    }
}
