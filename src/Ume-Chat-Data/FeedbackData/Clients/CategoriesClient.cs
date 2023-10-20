using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Data.FeedbackData;

namespace FeedbackData.Clients;

/// <summary>
///     Client handling categories in database.
/// </summary>
public class CategoriesClient
{
    private readonly FeedbackContext _context;
    private readonly ILogger<CategoriesClient> _logger;

    public CategoriesClient(FeedbackContext context, ILogger<CategoriesClient> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    ///     Retrieve all categories from database.
    /// </summary>
    /// <returns>List of all categories in database</returns>
    public async Task<List<Category>> GetCategoriesAsync()
    {
        try
        {
            return await _context.Categories.ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed CategoriesClient.GetCategoriesAsync()!");
            throw;
        }
    }

    /// <summary>
    ///     Retrieve category from database based on provided ID.
    /// </summary>
    /// <param name="id">ID of category</param>
    /// <returns>Category or null if not found</returns>
    public async Task<Category?> GetCategoryAsync(int id)
    {
        try
        {
            return await _context.Categories.FindAsync(id);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed CategoriesClient.GetCategoryAsync(int id)!");
            throw;
        }
    }
}
