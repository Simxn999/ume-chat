using Microsoft.AspNetCore.Mvc;
using Ume_Chat_Data_Feedback.Clients;
using Ume_Chat_Models.Data.FeedbackData;

namespace Ume_Chat_API_Feedback.Controllers;

/// <summary>
///     Statuses endpoints.
/// </summary>
[ApiController]
[Route("api/statuses")]
public class StatusesController : ControllerBase
{
    private readonly StatusesClient _client;
    private readonly ILogger<StatusesClient> _logger;

    public StatusesController(StatusesClient client, ILogger<StatusesClient> logger)
    {
        try
        {
            _client = client;
            _logger = logger;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed initialization of StatusesController!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve all statuses.
    /// </summary>
    /// <returns>List of all statuses.</returns>
    [HttpGet]
    public async Task<ActionResult<List<Status>>> GetStatusesAsync()
    {
        try
        {
            return await _client.GetStatusesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of all statuses!");
            return StatusCode(500, "Unknown error occured!");
        }
    }

    /// <summary>
    ///     Retrieve status based on ID.
    /// </summary>
    /// <param name="id">ID of status</param>
    /// <returns>Status based on ID</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Status>> GetStatusAsync(int id)
    {
        try
        {
            var status = await _client.GetStatusAsync(id);
            if (status is null)
                return NotFound();

            return status;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of status!");
            return StatusCode(500, "Unknown error occured!");
        }
    }
}
