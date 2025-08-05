using Sendify.Data;
using Sendify.MessageService;
using Sendify.Shared.Extensions;

namespace Sendify.MessageServiceSmsHttp;

public class MessageSenderSmsHttp : IMessageSender
{
    private readonly string _apiUrl;
    public MessageType ServiceType => MessageType.Sms;

    public MessageSenderSmsHttp(string apiUrl)
    {
        _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
    }

    public ResultMessage SendMessage(Message message)
    {
        return SendMessageAsync(message).Result;
    }

    public async Task<ResultMessage> SendMessageAsync(Message message)
    {
        if (message.Body == null || message.Body.Length == 0)
        {
            return new ResultMessage
            {
                IsSuccess = false,
                ErrorMessage = "Message body cannot be null or empty."
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

        var messages = message.Body.RemoveDiacritics().SplitByLength(160);

        foreach (var msg in messages)
        {
            foreach (var recipient in message.Recipients)
            {
                var result = await SendMessage(recipient, msg);

                if (recipient!= null && !result)
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

        if(phoneNumber.Substring(0,2) == "48")
        {
            phoneNumber = phoneNumber.Substring(2, phoneNumber.Length-2);
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