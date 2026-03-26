using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
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
