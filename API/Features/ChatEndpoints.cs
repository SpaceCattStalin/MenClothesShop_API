using Microsoft.AspNetCore.SignalR;
using Repositories.ApplicationDbContext;
using API.Hubs;
using Repositories.Models;
using Microsoft.EntityFrameworkCore;
using Common.Commons;

namespace API.Features
{
    public static class ChatEndpoints
    {
        public record SendMessageRequest(string content);
        public static void MapEndpoint(IEndpointRouteBuilder builder)
        {
            builder.MapPost("chat/send", async (
                 AppDbContext context,
                 IHubContext<Chathub> hubContext,
                 int userId,
                 SendMessageRequest request) =>
            {
                var now = Utils.UtcToLocalTimeZone(DateTime.UtcNow);

                // Find/create conversation
                var conversation = await context.Conversations
                    .FirstOrDefaultAsync(c => c.UserId == userId)
                    ?? new Conversation { UserId = userId, CreateAt = now };

                if (conversation.Id == 0)
                {
                    context.Conversations.Add(conversation);
                    await context.SaveChangesAsync();
                }


                var message = new Message
                {
                    ConversationId = conversation.Id,
                    Content = request.content,
                    SenderId = userId,
                    SenderType = SenderType.User,
                    SentAt = now
                };

                context.Messages.Add(message);
                conversation.UpdateAt = now;
                await context.SaveChangesAsync();

                await hubContext.Clients.Group("shop").SendAsync("ReceiveMessage", userId, request.content);

                return Results.Ok(message);
            });

            builder.MapGet("chat/messages", async (
               AppDbContext context,
               int userId,
               DateTime? since) =>
            {
                var query = context.Messages
                    .Where(m => m.Conversation.UserId == userId);

                if (since.HasValue)
                    query = query.Where(m => m.SentAt > since.Value);

                var messages = await query
                    .OrderBy(m => m.SentAt)
                    .Select(m => new
                    {
                        m.Id,
                        m.Content,
                        m.SenderId,
                        m.SenderType,
                        m.SentAt
                    })
                    .ToListAsync();

                return Results.Ok(messages);
            });

            builder.MapGet("shop/conversations", async (AppDbContext context) =>
            {
                var conversations = await context.Conversations
                    .Include(c => c.User)
                    .Select(c => new
                    {
                        c.Id,
                        c.UserId,
                        UserName = c.User.UserName,
                        c.UpdateAt,
                        LastMessage = c.Messages
                            .OrderByDescending(m => m.SentAt)
                            .Select(m => m.Content)
                            .FirstOrDefault()
                    })
                    .OrderByDescending(c => c.UpdateAt)
                    .ToListAsync();

                return Results.Ok(conversations);
            });

            builder.MapPost("shop/reply/{conversationId}", async (
               AppDbContext context,
               IHubContext<Chathub> hubContext,
               int conversationId,
               int shopUserId,
               SendMessageRequest request) =>
            {
                var conversation = await context.Conversations
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == conversationId);
                var now = Utils.UtcToLocalTimeZone(DateTime.UtcNow);

                if (conversation == null)
                    return Results.NotFound();

                var message = new Message
                {
                    ConversationId = conversationId,
                    Content = request.content,
                    SenderId = shopUserId,
                    SenderType = SenderType.Shop,
                    SentAt = now
                };

                context.Messages.Add(message);
                conversation.UpdateAt = now;
                await context.SaveChangesAsync();

                await hubContext.Clients.User(conversation.UserId.ToString())
                    .SendAsync("ReceiveMessage", shopUserId, request.content);

                return Results.Ok(message);
            });
        }
    }
}
