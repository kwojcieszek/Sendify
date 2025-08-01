namespace Sendify.MessageService;

public class ResultMessage
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }

    public ResultMessage()
    {

    }

    public ResultMessage(bool isSuccess, string errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}