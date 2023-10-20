using Microsoft.EntityFrameworkCore;
using TitleGeneratorGPT;
using Ume_Chat_Models.Data.FeedbackData;

namespace Ume_Chat_Data_Feedback.Data_Transfer;

/// <summary>
///     Type mapper extensions.
/// </summary>
public static class MapperExtensions
{
    /// <summary>
    ///     Maps FeedbackSubmissionDTO to FeedbackSubmission.
    /// </summary>
    /// <param name="dto">DTO to map</param>
    /// <param name="context">Database Context</param>
    /// <returns>FeedbackSubmission</returns>
    public static async Task<FeedbackSubmission> ToFeedbackSubmissionAsync(this FeedbackSubmissionDTO dto, FeedbackContext context)
    {
        var output = new FeedbackSubmission();

        output.Title = await GetTitleAsync(dto.Messages);
        output.Comment = dto.Comment;
        output.Date = GetDate();
        output.Messages = GetMessages(output.ID, dto.Messages);
        output.StatusID = 1;
        output.Status = await GetStatusAsync(1, context);
        output.Categories = await GetCategoriesAsync(dto.CategoryIDs, context);

        return output;
    }

    /// <summary>
    ///     Generate a feedback submission title based on the message history.
    /// </summary>
    /// <param name="messages">Message history</param>
    /// <returns>String with generated title</returns>
    private static async Task<string> GetTitleAsync(ICollection<MessageDTO> messages)
    {
        // Retrieve the message before the marked response
        var message = messages.ElementAt(messages.Count - 2);

        return await TitleGenerator.GenerateTitleAsync(message.Content);
    }

    /// <summary>
    ///     <para>Retrieve the current date with precision zero.</para>
    ///     <para>Database does not use subseconds so when returning the submitted object without trimming subseconds, dates do not match.</para> 
    /// </summary>
    /// <returns>DateTimeOffset.Now with zero precision</returns>
    private static DateTimeOffset GetDate()
    {
        var date = DateTimeOffset.Now;
        
        // Return new DateTimeOffset with no subseconds
        return new DateTimeOffset(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Offset);
    }

    /// <summary>
    ///     Convert message data transfer objects to messages.
    /// </summary>
    /// <param name="parent">ID of FeedbackSubmission</param>
    /// <param name="messageDTOs">Message data transfer objects</param>
    /// <returns>List of messages</returns>
    private static List<Message> GetMessages(Guid parent, IEnumerable<MessageDTO> messageDTOs)
    {
        var messages = new List<Message>();
        var messagePosition = 0;

        foreach (var messageDTO in messageDTOs)
        {
            messagePosition++;
            var message = new Message();

            message.Role = messageDTO.Role;
            message.Content = messageDTO.Content;
            message.Position = messagePosition;
            message.Citations = GetCitations(message.ID, messageDTO.Citations);
            message.FeedbackSubmissionID = parent;

            messages.Add(message);
        }

        return messages;
    }

    /// <summary>
    ///     Convert citation data transfer objects to citations.
    /// </summary>
    /// <param name="parent">ID of Message</param>
    /// <param name="citationDTOs">Citation data transfer objects</param>
    /// <returns>List of citations</returns>
    private static List<Citation> GetCitations(Guid parent, IEnumerable<CitationDTO> citationDTOs)
    {
        return citationDTOs.Select(citationDTO =>
                            {
                                var citation = new Citation();

                                citation.DocumentID = citationDTO.DocumentID;
                                citation.TextID = citationDTO.TextID;
                                citation.Position = citationDTO.Position;
                                citation.MessageID = parent;

                                return citation;
                            })
                           .ToList();
    }

    /// <summary>
    ///     Retrieve status from database based on ID.
    /// </summary>
    /// <param name="id">ID of status</param>
    /// <param name="context">Database Context</param>
    /// <returns>Status</returns>
    private static async Task<Status> GetStatusAsync(int id, FeedbackContext context)
    {
        var status = await context.Statuses.FindAsync(id);

        ArgumentNullException.ThrowIfNull(status);

        return status;
    }

    /// <summary>
    ///     Retrieve categories from database based on collection of IDs.
    /// </summary>
    /// <param name="categoryIDs">Collection of Category IDs</param>
    /// <param name="context">Database Context</param>
    /// <returns>Collection of categories</returns>
    private static async Task<ICollection<Category>> GetCategoriesAsync(ICollection<int> categoryIDs, FeedbackContext context)
    {
        return await context.Categories.Where(c => categoryIDs.Contains(c.ID)).ToListAsync();
    }
}
