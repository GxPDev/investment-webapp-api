using System.Collections.Generic;

namespace InvestmentWebApp.Models
{
    public class DashboardViewModel
    {
        public AppUser User { get; set; } = null!;
        public List<Transaction> Transactions { get; set; } = new();
        public List<WithdrawalRequest> WithdrawalRequests { get; set; } = new();
        public List<DepositRequest> DepositRequests { get; set; } = new();
        public List<TransferRequest> SentTransfers { get; set; } = new();
        public List<TransferRequest> ReceivedTransfers { get; set; } = new();
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> ChartData { get; set; } = new();
        public List<decimal> BalanceData { get; set; } = new();
        public decimal DailyReturn { get; set; }
        public DateTime LastProfitDate { get; set; }
        public int TransactionPage { get; set; }
        public int TransactionTotalPages { get; set; }
    }
}
