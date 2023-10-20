using Microsoft.AspNetCore.Mvc;
using FeedbackData.Clients;
using Models.Data.FeedbackData;

namespace FeedbackAPI.Controllers;

/// <summary>
///     Categories endpoints.
/// </summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly CategoriesClient _client;
    private readonly ILogger<CategoriesClient> _logger;

    public CategoriesController(CategoriesClient client, ILogger<CategoriesClient> logger)
    {
        try
        {
            _client = client;
            _logger = logger;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed initialization of CategoriesController!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve all categories.
    /// </summary>
    /// <returns>List of all categories.</returns>
    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetCategoriesAsync()
    {
        try
        {
            return await _client.GetCategoriesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of all Categories!");
            return StatusCode(500, "Unknown error occured!");
        }
    }

    /// <summary>
    ///     Retrieve category based on ID.
    /// </summary>
    /// <param name="id">ID of category</param>
    /// <returns>Category based on ID</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Category>> GetCategoryAsync(int id)
    {
        try
        {
            var category = await _client.GetCategoryAsync(id);

            if (category is null)
                return NotFound();

            return category;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed retrieval of Category!");
            return StatusCode(500, "Unknown error occured!");
        }
    }
}
