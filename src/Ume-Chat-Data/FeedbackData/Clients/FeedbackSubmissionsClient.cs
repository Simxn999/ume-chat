using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ume_Chat_Data_Feedback.Data_Transfer;
using Ume_Chat_Models.Data.FeedbackData;

namespace Ume_Chat_Data_Feedback.Clients;

/// <summary>
///     Client handling feedback submissions in database.
/// </summary>
public class FeedbackSubmissionsClient
{
    private readonly FeedbackContext _context;
    private readonly ILogger<FeedbackSubmissionsClient> _logger;

    public FeedbackSubmissionsClient(FeedbackContext context, ILogger<FeedbackSubmissionsClient> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieve all feedback submissions from databsae.
    /// </summary>
    /// <returns>List of all feedback submissions in database</returns>
    public async Task<List<FeedbackSubmission>> GetFeedbackSubmissionsAsync()
    {
        try
        {
            return await _context.FeedbackSubmissions.Include(x => x.Status)
                                 .Include(x => x.Messages.OrderBy(m => m.Position))
                                 .ThenInclude(x => x.Citations)
                                 .Include(x => x.Categories)
                                 .ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed FeedbackSubmissionsClient.GetCategoriesAsync()!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve feedback submission from database based on ID.
    /// </summary>
    /// <param name="id">ID of feedback submission</param>
    /// <returns>FeedbackSubmission or null if not found</returns>
    public async Task<FeedbackSubmission?> GetFeedbackSubmissionAsync(Guid id)
    {
        try
        {
            return await _context.FeedbackSubmissions.Include(x => x.Status)
                                 .Include(x => x.Messages.OrderBy(m => m.Position))
                                 .ThenInclude(x => x.Citations)
                                 .Include(x => x.Categories)
                                 .SingleOrDefaultAsync(x => x.ID == id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed FeedbackSubmissionsClient.GetFeedbackSubmissionAsync(Guid id)!");
            throw;
        }
    }

    /// <summary>
    ///     Upload given feedback submission to database.
    /// </summary>
    /// <param name="dto">Feedback Submission Data Transfer Object</param>
    /// <returns>FeedbackSubmission</returns>
    public async Task<FeedbackSubmission> UploadFeedbackSubmissionAsync(FeedbackSubmissionDTO dto)
    {
        try
        {
            var feedbackSubmission = await dto.ToFeedbackSubmissionAsync(_context);

            await _context.FeedbackSubmissions.AddAsync(feedbackSubmission);
            await _context.SaveChangesAsync();

            return feedbackSubmission;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed submitting feedback!");
            throw;
        }
    }

    /// <summary>
    ///     Delete feedback submission in database based on ID.
    /// </summary>
    /// <param name="id">ID of feedback submission</param>
    /// <returns>Boolean if deletion was successfull or not</returns>
    public async Task<bool> DeleteFeedbackSubmissionAsync(Guid id)
    {
        try
        {
            var feedbackSubmission = await _context.FeedbackSubmissions.FindAsync(id);
            if (feedbackSubmission is null)
                return false;

            _context.FeedbackSubmissions.Remove(feedbackSubmission);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed submitting feedback!");
            throw;
        }
    }
}
