using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Commons
{
    public enum ErrorCode
    {
        // Default
        UnDefinedError,

        // Authentication & Authorization
        EmailNotVerified,
        InvalidCredentials,
        AccountLocked,
        AccessDenied,
        InvalidToken,
        TokenRequired,

        // Validation
        ValidationError,
        MissingRequiredField,
        InvalidEmailFormat,
        PasswordTooWeak,

        // User Management
        UserNotFound,
        UserAlreadyExists,
        InvalidUserRole,

        // Resource Management
        ResourceNotFound,
        ResourceAlreadyExists,
        ResourceLimitExceeded,

        // System & Server
        InternalServerError,
        ServiceUnavailable,
        DatabaseError,
        NetworkError,

        // Payment & Subscription
        PaymentFailed,
        SubscriptionExpired,
        InvalidPaymentMethod,

        // Third-Party Integration
        ThirdPartyApiError,
        ExternalServiceUnavailable,

        // Custom Business Logic
        InvalidOperation,
        QuotaExceeded,
        InvalidInvitationCode,

        InvalidSize,
        InsufficientStock
    }
}
