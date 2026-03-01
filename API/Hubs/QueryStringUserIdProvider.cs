using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    /// <summary>
    /// Maps SignalR connections to user id from query string "userId".
    /// Enables Clients.User(userId) to reach the correct client (e.g. for chat).
    /// </summary>
    public class QueryStringUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var ctx = connection.GetHttpContext();
            if (ctx?.Request.Query.TryGetValue("userId", out var value) == true && value.Count > 0)
                return value[0];
            return null;
        }
    }
}
