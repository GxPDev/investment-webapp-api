using Microsoft.EntityFrameworkCore;
using InvestmentWebApp.Models;

namespace InvestmentWebApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

        public DbSet<AppUser> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }
        public DbSet<DepositRequest> DepositRequests { get; set; }
        public DbSet<TransferRequest> TransferRequests { get; set; }
        public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
    }
}
