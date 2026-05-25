using System;

namespace InvestmentWebApp.Models
{
    public class TransferRequest
    {
        public int Id { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string RecipientUsername { get; set; } = string.Empty;
        public string TransferCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal FeeAmount { get; set; }
        public string? Note { get; set; }
        public bool RecipientConfirmed { get; set; }
        public DateTime? RecipientConfirmedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}
