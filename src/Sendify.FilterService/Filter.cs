using System.Text.RegularExpressions;
using Sendify.Data;

namespace Sendify.FilterService;

public class Filter : IFilter
{
    private readonly Pattern[] _patterns;

    public Filter(Pattern[] patterns)
    {
        _patterns = patterns;
    }

    public FilterResult IsMessageAllowed(Message message)
    {
        bool senderResult = false;

        if (message.Recipients is null)
        {
            return new FilterResult()
            {
                IsAllowed = false,
                Reason = "Message has no recipients."
            };
        }

        foreach (var filterMessage in _patterns.Where(f => f.MessageType == message.MessageType && f.IsSender))
        {
            if (Regex.IsMatch(message.Sender, filterMessage.Value, RegexOptions.IgnoreCase))
            {
                senderResult = true;
                break;
            }
        }

        if (!senderResult)
        {
            return new FilterResult()
            {
                IsAllowed = false,
                Reason = $"Sender '{message.Sender}' is not allowed."
            };
        }

        var recipientFilterMessages = _patterns.Where(f => f.MessageType == message.MessageType && f.IsRecipient);

        foreach (var recipient in message.Recipients)
        {
            bool recipientResult = false;

            foreach (var filterMessage in recipientFilterMessages)
            {
                if (Regex.IsMatch(recipient, filterMessage.Value, RegexOptions.IgnoreCase))
                {
                    recipientResult = true;
                    break;
                }
            }

            if (!recipientResult)
            {
                return new FilterResult()
                {
                    IsAllowed = false,
                    Reason = $"Recipient '{recipient}' is not allowed."
                };
            }
        }

        return new FilterResult()
        {
            IsAllowed = true
        };
    }
}