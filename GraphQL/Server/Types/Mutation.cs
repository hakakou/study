using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Types;

public class Mutation
{
    public AddCommentPayload AddComment(AddCommentInput input)
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

    [UseMutationConvention()]
    [Error(typeof(UserNameTakenError))]
    public Book CreateUser([ID] string username)
    {
        throw new UserNameTakenException(username);
        //return new Book(title, new Author(author));

        //The name of the exception will be rewritten.
        //Exception is replaced with Error to follow the common GraphQL naming conventions.
    }
}

//public class CreateUserErrorFactory(ILogger<CreateUserErrorFactory> logger)
//    : IPayloadErrorFactory<UserNameTakenException, UserNameTakenError>
//{
//    public UserNameTakenError CreateErrorFrom(UserNameTakenException ex)
//    {
//        return new UserNameTakenError(ex.Username);
//    }
//}

public class UserNameTakenException : Exception
{
    public string Username { get; }
    public UserNameTakenException(string username)
        : base($"The username {username} is already taken.")
    {
        Username = username;
    }
}

[GraphQLName("UserError")]
public interface IUserError
{
    string Message { get; }
    string Code { get; }
}

//public class UserNameTakenError
//{
//    public UserNameTakenError(string username)
//    {
//        Message = $"The username {username} is already taken. Use a different username.";
//    }

//    public static UserNameTakenError CreateErrorFrom(UserNameTakenException ex)
//    {
//        return new UserNameTakenError(ex.Username);
//    }

//    public string Message { get; }
//}

public class UserNameTakenError : IUserError
{
    public UserNameTakenError(UserNameTakenException ex)
    {
        Message = $"The username {ex.Username} is already taken. Use a different username.";
        Code = "USER_NAME_TAKEN";
    }
    public string Code { get; }
    public string Message { get; }
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
