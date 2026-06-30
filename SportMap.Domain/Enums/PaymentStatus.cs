namespace SportMap.Domain.Enums;

public enum PaymentStatus
{
    Unpaid,
    PendingVerification,  // ← جديد: العميل بعت رقم مرجعي ومستني تأكيد
    Paid
}