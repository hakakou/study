using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Types;

public class Mutation
{
    public AddCommentPayload AddItemsToPlaylist(AddCommentInput input)
    {
        try
        {
            // Here you would typically add the comment to a database or data store.
            var comment = new Comment(42, input.Text);

            return new AddCommentPayload(200, true, "Added item successfully", comment);
        }
        catch (Exception ex)
        {
            return new AddCommentPayload(500, false, $"Failed to add item: {ex.Message}", null);
        }
    }
}

public class AddCommentInput
{
    [ID]
    public int ArticleId { get; set; }
    public string Text { get; set; }
    public AddCommentInput(int articleId, string text)
    {
        ArticleId = articleId;
        Text = text;
    }
}

public class AddCommentPayload
{
    [GraphQLDescription("Similar to HTTP status code, represents the status of the mutation.")]
    public int Code { get; set; }

    [GraphQLDescription("Indicates whether the mutation was successful.")]
    public bool Success { get; set; }

    [GraphQLDescription("Human-readable message for the UI.")]
    public string Message { get; set; }

    public Comment? Comment { get; set; }

    public AddCommentPayload(int code, bool success, string message, Comment? comment)
    {
        Code = code;
        Success = success;
        Message = message;
        Comment = comment;
    }
}
