using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ume_Chat_Models.Data.FeedbackData;

namespace Ume_Chat_Data_Feedback.Clients;

/// <summary>
///     Client handling statuses in database.
/// </summary>
public class StatusesClient
{
    private readonly FeedbackContext _context;
    private readonly ILogger<StatusesClient> _logger;

    public StatusesClient(FeedbackContext context, ILogger<StatusesClient> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieve all statuses from database.
    /// </summary>
    /// <returns>List of all statuses from database</returns>
    public async Task<List<Status>> GetStatusesAsync()
    {
        try
        {
            return await _context.Statuses.ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed StatusesClient.GetStatusesAsync()!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve status from database based on ID.
    /// </summary>
    /// <param name="id">ID of status</param>
    /// <returns>Status or null if not found</returns>
    public async Task<Status?> GetStatusAsync(int id)
    {
        try
        {
            return await _context.Statuses.FindAsync(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed StatusesClient.GetStatusAsync(int id)!");
            throw;
        }
    }
}
