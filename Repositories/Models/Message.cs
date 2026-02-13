using System;

namespace Repositories.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }
        public string Content { get; set; }
        public int SenderId { get; set; }
        public User Sender { get; set; }
        public SenderType SenderType { get; set; }
        public DateTime SentAt { get; set; }
    }

    public enum SenderType
    {
        User = 1,
        Shop = 2
    }
}
