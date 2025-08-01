using Microsoft.AspNetCore.Mvc;

namespace Sendify.Api.Extensions;

public static class ControllerBaseExtensions
{
    public static string? GetGroupId(this ControllerBase controller)
    {
        var groupId = controller.User.GroupId();

        if (groupId == null)
        {
            controller.Response.StatusCode = 404;

            return null;
        }
        else
        {
            return groupId;
        }
    }

    public static string? GetUserId(this ControllerBase controller)
    {
        var userId = controller.User.UserId();

        if (userId == null)
        {
            controller.Response.StatusCode = 404;

            return null;
        }
        else
        {
            return userId;
        }
    }
}