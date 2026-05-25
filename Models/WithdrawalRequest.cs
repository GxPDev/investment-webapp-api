using System;

namespace InvestmentWebApp.Models
{
    public class WithdrawalRequest
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string WalletAddress { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }
        public string? ApprovalCode { get; set; }
        public bool ApprovalCodeRevealed { get; set; }
        public DateTime? ApprovalCodeRevealedAt { get; set; }
        public DateTime? ProcessingAt { get; set; }
        public DateTime? FinalApprovedAt { get; set; }
    }
}
