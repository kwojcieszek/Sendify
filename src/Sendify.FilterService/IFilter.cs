using Sendify.Data;

namespace Sendify.FilterService;

public interface IFilter
{
    FilterResult IsMessageAllowed(Message message);
}