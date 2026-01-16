using Sendify.Data;
using Sendify.MessagesService;
using Sendify.Shared.Extensions;

namespace Sendify.MessagesServiceSmsHttp;

public class MessagesSenderSmsHttp : IMessagesSender
{
    private readonly string _apiUrl;
    public MessageType ServiceType => MessageType.Sms;
    public string Sender => string.Empty;

    public MessagesSenderSmsHttp(string apiUrl)
    {
        _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
    }

    public ResultMessage SendMessage(Message message)
    {
        return SendMessageAsync(message).Result;
    }

    public async Task<ResultMessage> SendMessageAsync(Message message)
    {
        if (message.Body.Length == 0)
        {
            return new ResultMessage
            {
                IsSuccess = false,
                ErrorMessage = "Message body cannot be empty."
            };
        }

        if (message.Recipients == null || message.Recipients.Count == 0)
        {
            return new ResultMessage
            {
                IsSuccess = false,
                ErrorMessage = "No recipients specified."
            };
        }

        var messages = message.Body.Normalize().SplitByLength(160);

        foreach (var msg in messages)
        {
            foreach (var recipient in message.Recipients)
            {
                var result = await SendMessage(recipient, msg);

                if (!result)
                {
                    return new ResultMessage
                    {
                        IsSuccess = false,
                        ErrorMessage = $"Failed to send message to {recipient}"
                    };
                }

                await Task.Delay(200);
            }
        }

        return new ResultMessage
        {
            IsSuccess = true
        };
    }

    private async Task<bool> SendMessage(string phoneNumber, string messageText)
    {
        var httpClient = new HttpClient();

        if(phoneNumber[..2] == "48")
        {
            phoneNumber = phoneNumber[2..];
        }

        var response = await httpClient.GetAsync($"{_apiUrl}/sms.php?phonenumber={phoneNumber}&message={Uri.EscapeDataString(messageText)}");

        if (response is { IsSuccessStatusCode: true, StatusCode: System.Net.HttpStatusCode.OK })
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}