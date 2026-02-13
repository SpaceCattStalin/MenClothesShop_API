using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class Chathub : Hub
    {
        // Test function
        public async Task NewMessage(long username, string message) =>
            await Clients.All.SendAsync("MessageReceived", username, message);

        public async Task SendMessageToShop(string userId, string message)
        {
            await Clients.Group("shop").SendAsync("ReceiveMessage", userId, message);
        }

        public async Task SendMessageToUser(int userId, string message)
        {
            await Clients.User(userId.ToString()).SendAsync("ReceiveMessage", userId, message);
        }

        public async Task JoinShopGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "shop");
        }
    }
}
