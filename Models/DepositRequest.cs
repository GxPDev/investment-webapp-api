using System;

namespace InvestmentWebApp.Models
{
    public class DepositRequest
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionReason { get; set; }
    }
}
