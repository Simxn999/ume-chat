using Microsoft.AspNetCore.Mvc;
using Ume_Chat_Data_Feedback.Clients;
using Ume_Chat_Data_Feedback.Data_Transfer;
using Ume_Chat_Models.Data.FeedbackData;

namespace Ume_Chat_API_Feedback.Controllers;

/// <summary>
///     Feedback submissions endpoints.
/// </summary>
[ApiController]
[Route("api/feedback-submissions")]
public class FeedbackSubmissionsController : ControllerBase
{
    private readonly FeedbackSubmissionsClient _client;
    private readonly ILogger<FeedbackSubmissionsClient> _logger;

    public FeedbackSubmissionsController(FeedbackSubmissionsClient client, ILogger<FeedbackSubmissionsClient> logger)
    {
        try
        {
            _client = client;
            _logger = logger;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed initialization of FeedbackSubmissionsController!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve all feedback submissions.
    /// </summary>
    /// <returns>List of feedback submissions</returns>
    [HttpGet]
    public async Task<ActionResult<List<FeedbackSubmission>>> GetFeedbackSubmissionsAsync()
    {
        try
        {
            return await _client.GetFeedbackSubmissionsAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of all FeedbackSubmissions!");
            return StatusCode(500, "Unknown error occured!");
        }
    }

    /// <summary>
    ///     Retrieve feedback submission based on ID.
    /// </summary>
    /// <param name="id">ID of feedback submission</param>
    /// <returns>Feedback submission</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FeedbackSubmission>> GetFeedbackSubmissionAsync(Guid id)
    {
        try
        {
            var feedbackSubmission = await _client.GetFeedbackSubmissionAsync(id);

            if (feedbackSubmission is null)
                return NotFound();

            return feedbackSubmission;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of FeedbackSubmission!");
            return StatusCode(500, "Unknown error occured!");
        }
    }

    /// <summary>
    ///     Submit a feedback submission.
    /// </summary>
    /// <param name="dto">Feedback submission data transfer object</param>
    /// <returns>Feedback submission</returns>
    [HttpPost]
    public async Task<ActionResult<FeedbackSubmission>> UploadFeedbackSubmissionAsync([FromBody] FeedbackSubmissionDTO dto)
    {
        try
        {
            var feedbackSubmission = await _client.UploadFeedbackSubmissionAsync(dto);

            return CreatedAtAction("GetFeedbackSubmission", new { id = feedbackSubmission.ID }, feedbackSubmission);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed submission of FeedbackSubmission!");
            return StatusCode(500, "Unknown error occured!");
        }
    }

    /// <summary>
    ///     Delete feedback submission based on ID.
    /// </summary>
    /// <param name="id">ID of feedback submission</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFeedbackSubmissionAsync(Guid id)
    {
        try
        {
            if (!await _client.DeleteFeedbackSubmissionAsync(id))
                return NotFound();

            return NoContent();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of FeedbackSubmission!");
            return StatusCode(500, "Unknown error occured!");
        }
    }
}
