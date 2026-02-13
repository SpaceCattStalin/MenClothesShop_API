using API.Interfaces;
using Common.Commons;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using Repositories.ApplicationDbContext;
using Repositories.Models;
using Sprache;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace API.Services
{
    public class PaymentService
    {
        private readonly PayOSClient _payOS;
        private readonly ILogger<PaymentService> _logger;
        private readonly AppDbContext _context;
        private readonly ICartService _cartService;

        public PaymentService(ILogger<PaymentService> logger, AppDbContext context, ICartService cartService)
        {
            _payOS = new PayOSClient(
                 Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID"),
                 Environment.GetEnvironmentVariable("PAYOS_API_KEY"),
                 Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY")
            );
            _logger = logger;
            _context = context;
            _cartService = cartService;
        }

        public async Task<CreatePaymentLinkResponse> CreatePaymentRequest(Order order)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == order.UserId);

                // Create payment record (not saved yet)
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = order.Total,
                    Created = Utils.UtcToLocalTimeZone(DateTime.UtcNow),
                    PaymentType = PaymentStatus.Pending
                };
                var cancelUrl = "http://example.com";
                var returnUrl = "http://example.com";
                var buyerEmail = user.UserName;
                var buyerName = user.UserName;

                var query = $"amount={order.Total}" +
                    $"$cancelUrl={cancelUrl}&returnUrl={returnUrl}" +
                    $"&description=Order #{order.Id} for User #{user.UserId}&orderCode=123" +
                    $"&buyerEmail={buyerEmail}&buyerName={buyerName}";
                // Call PayOS first
                var request = new CreatePaymentLinkRequest
                {
                    OrderCode = order.Id,
                    Amount = (long)order.Total,
                    Description = $"Order #{order.Id} for User #{user.UserId}",
                    BuyerEmail = user.UserName,
                    BuyerName = user.UserName,
                    CancelUrl = "http://example.com",
                    ReturnUrl = "http://example.com",
                    Signature = CreateSignature(query)
                };

                var link = await _payOS.PaymentRequests.CreateAsync(request);

                // PayOS succeeded - now save payment with transaction ID
                payment.TransactionId = link.PaymentLinkId;
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return link;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<ConfirmWebhookResponse> RegisterWebhook()
        {
            var confirmedWebhook = await _payOS.Webhooks.ConfirmAsync("https://curious-pauline-catchable.ngrok-free.dev/payos/webhook");

            return confirmedWebhook;
        }

        public async Task<WebhookData> VerifyWebhook(Webhook webhook)
        {
            var verifiedData = await _payOS.Webhooks.VerifyAsync(webhook);
            try
            {
                if (verifiedData.Code.Equals("00"))
                {
                    var orderId = (int)verifiedData.OrderCode;

                    var payment = await _context.Payments
                        .FirstOrDefaultAsync(p => p.OrderId == orderId);

                    payment.PaymentType = PaymentStatus.Completed;

                    var match = Regex.Match(verifiedData.Description, @"User\s+#?\d+");

                    if (match.Success)
                    {
                        var userId = match.Value;
                        await _cartService.ClearCart(int.Parse(userId.Split(' ')[1]));
                    }
                }
                await _context.SaveChangesAsync();
                return verifiedData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return verifiedData;
            }
        }

        private string CreateSignature(string query)
        {
            var checksumKey = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY");

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey));
            byte[] data = hmac.ComputeHash(Encoding.UTF8.GetBytes(query));
            var signature = Convert.ToHexString(data);

            return signature;
        }
    }
}
