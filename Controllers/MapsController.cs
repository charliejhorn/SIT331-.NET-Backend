using Microsoft.AspNetCore.Mvc;
using robot_controller_api.Persistence;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

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
    [HttpGet("square"), AllowAnonymous]
    public IEnumerable<Map> GetAllSquareMaps()
    {
        List<Map> maps = _mapsRepo.GetMaps();
        return maps.Where(map => map.IsSquare);
    }

    /// <summary>
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
    [HttpGet("{id}", Name = "GetMap"), AllowAnonymous]
    public IActionResult GetMapById(int id)
    {
        List<Map> maps = _mapsRepo.GetMaps();

        Map? map = maps.Find(map => map.Id == id);

        if(map == null) return NotFound();

        return Ok(map);
    }

    /// <summary>
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
    /// <response code="409">If a map with the same name already exists.</response>
    [HttpPost(), Authorize(Policy = "AdminOnly")]
    public IActionResult AddMap(Map newMap)
    {
        if(newMap == null) return BadRequest();

        List<Map> maps = _mapsRepo.GetMaps();
        if(maps.Exists(map => map.Name == newMap.Name)) return Conflict("Name already exists.");

        newMap.CreatedDate = DateTime.Now;
        newMap.ModifiedDate = DateTime.Now;

        Map? addedMap = _mapsRepo.AddMap(newMap);

        if (addedMap == null) return BadRequest("Map could not be created.");

        return CreatedAtRoute("GetMap", new { id = addedMap.Id }, addedMap);
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
    ///     POST /api/maps
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
    /// <response code="404">If a map with the provided id can't be found.</response>
    /// <response code="409">If a map with the same name as the new name provided already exists.</response>
    [HttpPut("{id}"), Authorize(Policy = "AdminOnly")]
    public IActionResult UpdateMap(int id, Map updatedMap)
    {
        if (updatedMap == null) return BadRequest("Map data is required.");

        // check map exists
        List<Map> maps = _mapsRepo.GetMaps();
        Map? existingMap = maps.Find(map => map.Id == id);
        if (existingMap == null) return NotFound();

        // check name doesn't already exist
        if(maps.Exists(map => map.Id != id && map.Name == updatedMap.Name)) return Conflict("Name already exists.");


        updatedMap.Id = id;
        updatedMap.ModifiedDate = DateTime.UtcNow;
        updatedMap.CreatedDate = existingMap.CreatedDate; // keep original creation date

        // push change to database
        Map? returnedMap = _mapsRepo.UpdateMap(updatedMap);

        if (returnedMap == null) return BadRequest("Map could not be updated.");

        return Ok(returnedMap);
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
    /// <response code="404">If a map with the provided ID can't be found.</response>
    [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
    public IActionResult DeleteMap(int id)
    {
        bool success = _mapsRepo.DeleteMap(id);

        if (success) return NoContent();
        else return NotFound();
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
    /// <response code="404">If a map with the provided ID can't be found.</response>
    [HttpGet("{id}/{x}-{y}"), Authorize(Policy = "UserOnly")]
    public IActionResult GetCoordinate(int id, int x, int y)
    {
        List<Map> maps = _mapsRepo.GetMaps();
        Map? targetMap = maps.Find((Map map) => map.Id == id);
        if (targetMap == null) return NotFound();

        bool xValid = 0 <= x && x < targetMap.Columns;
        bool yValid = 0 <= y && y < targetMap.Rows;

        bool isOnMap = xValid && yValid;
        
        return Ok(isOnMap);
    }

    /// <summary>
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
    /// <response code="404">If a map with the provided ID can't be found.</response>
    /// <response code="409">If a map with the same name as the new name provided already exists.</response>
    [HttpPatch("{id}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> PatchMap(int id)
    {
        // fetch existing map
        List<Map> maps = _mapsRepo.GetMaps();
        Map? existingMap = maps.Find(map => map.Id == id);
        if (existingMap == null) return NotFound();

        // enable buffering so we can read the request body
        Request.EnableBuffering();
        Request.Body.Position = 0;
        
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var requestBody = await reader.ReadToEndAsync();
        
        try {
            // parse the JSON to see which fields were included
            using JsonDocument doc = JsonDocument.Parse(requestBody);
            JsonElement root = doc.RootElement;

            // for each field included, make change to existingMap
            if (root.TryGetProperty("columns", out JsonElement columnsElement))
            {
                existingMap.Columns = columnsElement.GetInt32();
            }

            if (root.TryGetProperty("rows", out JsonElement rowsElement))
            {
                existingMap.Rows = rowsElement.GetInt32();
            }
            
            if (root.TryGetProperty("name", out JsonElement nameElement))
            {
                existingMap.Name = nameElement.GetString();
            }
            
            if (root.TryGetProperty("description", out JsonElement descElement))
            {
                existingMap.Description = descElement.ValueKind == JsonValueKind.Null ? 
                    null : descElement.GetString();
            }
            
            // check name doesn't exist
            if(maps.Exists(map => map.Id != id && map.Name == existingMap.Name)) return Conflict("Name already exists.");

            existingMap.ModifiedDate = DateTime.UtcNow;
            
            // save to database
            Map? returnedMap = _mapsRepo.UpdateMap(existingMap);

            if (returnedMap == null) return BadRequest("Map could not be patched.");

            return Ok(returnedMap);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            return BadRequest("Invalid JSON format");
        }
    }
}
