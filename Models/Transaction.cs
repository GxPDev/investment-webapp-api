using System;

namespace InvestmentWebApp.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Type { get; set; } = string.Empty;

        public string? WalletAddress { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
