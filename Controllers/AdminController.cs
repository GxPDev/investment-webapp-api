using Microsoft.AspNetCore.Mvc;
using InvestmentWebApp.Data;
using System.Linq;

namespace InvestmentWebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 10, int usersPage = 1, int depositsPage = 1, int transfersPage = 1, int withdrawalsPage = 1, int usersPageSize = 10, int depositsPageSize = 10, int transfersPageSize = 10, int withdrawalsPageSize = 10)
        {
            var role = HttpContext.Session.GetString("role");
            if (role != "Admin")
                return RedirectToAction("Index", "Dashboard");

            var allowedPageSizes = new[] { 5, 10, 20, 50 };
            if (!allowedPageSizes.Contains(pageSize))
                pageSize = 10;

            if (!allowedPageSizes.Contains(usersPageSize))
                usersPageSize = 10;
            if (!allowedPageSizes.Contains(depositsPageSize))
                depositsPageSize = 10;
            if (!allowedPageSizes.Contains(transfersPageSize))
                transfersPageSize = 10;
            if (!allowedPageSizes.Contains(withdrawalsPageSize))
                withdrawalsPageSize = 10;

            var usersQuery = _context.Users.OrderBy(u => u.Username);
            var usersTotal = usersQuery.Count();
            var usersTotalPages = (int)Math.Ceiling(usersTotal / (double)usersPageSize);
            var usersCurrentPage = Math.Max(1, Math.Min(usersPage, usersTotalPages == 0 ? 1 : usersTotalPages));
            var users = usersQuery
                .Skip((usersCurrentPage - 1) * usersPageSize)
                .Take(usersPageSize)
                .ToList();

            var withdrawalQuery = _context.WithdrawalRequests
                .OrderByDescending(w => w.RequestedAt);
            var withdrawalTotal = withdrawalQuery.Count();
            var withdrawalTotalPages = (int)Math.Ceiling(withdrawalTotal / (double)withdrawalsPageSize);
            var withdrawalCurrentPage = Math.Max(1, Math.Min(withdrawalsPage, withdrawalTotalPages == 0 ? 1 : withdrawalTotalPages));
            var withdrawalRequests = withdrawalQuery
                .Skip((withdrawalCurrentPage - 1) * withdrawalsPageSize)
                .Take(withdrawalsPageSize)
                .ToList();

            var depositQuery = _context.DepositRequests
                .OrderByDescending(d => d.RequestedAt);
            var depositTotal = depositQuery.Count();
            var depositTotalPages = (int)Math.Ceiling(depositTotal / (double)depositsPageSize);
            var depositCurrentPage = Math.Max(1, Math.Min(depositsPage, depositTotalPages == 0 ? 1 : depositTotalPages));
            var depositRequests = depositQuery
                .Skip((depositCurrentPage - 1) * depositsPageSize)
                .Take(depositsPageSize)
                .ToList();

            var transferQuery = _context.TransferRequests
                .OrderByDescending(t => t.RequestedAt);
            var transferTotal = transferQuery.Count();
            var transferTotalPages = (int)Math.Ceiling(transferTotal / (double)transfersPageSize);
            var transferCurrentPage = Math.Max(1, Math.Min(transfersPage, transferTotalPages == 0 ? 1 : transferTotalPages));
            var transferRequests = transferQuery
                .Skip((transferCurrentPage - 1) * transfersPageSize)
                .Take(transfersPageSize)
                .ToList();
            var auditLogs = _context.AdminAuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .ToList();
            var dailyProfitQuery = _context.Transactions
                .Where(t => t.Type == "Daily Profit");

            if (startDate.HasValue)
                dailyProfitQuery = dailyProfitQuery.Where(t => t.CreatedAt.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                dailyProfitQuery = dailyProfitQuery.Where(t => t.CreatedAt.Date <= endDate.Value.Date);

            var totalAuditCount = dailyProfitQuery.Count();
            var totalPages = (int)Math.Ceiling(totalAuditCount / (double)pageSize);
            var currentPage = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var dailyProfitAudit = dailyProfitQuery
                .OrderByDescending(t => t.CreatedAt)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pageTotal = dailyProfitAudit.Sum(t => t.Amount);
            var filteredTotal = dailyProfitQuery.Sum(t => t.Amount);

            ViewBag.TotalUsers = usersTotal;
            ViewBag.TotalBalance = _context.Users.Sum(u => u.Balance);
            ViewBag.TotalInvestments = _context.Transactions
                .Where(t => t.Type == "Investment")
                .Sum(t => (decimal?)t.Amount) ?? 0;
            ViewBag.TotalWithdrawals = _context.Transactions
                .Where(t => t.Type == "Withdrawal")
                .Sum(t => (decimal?)t.Amount) ?? 0;
            ViewBag.TotalProfits = _context.Transactions
                .Where(t => t.Type == "Daily Profit")
                .Sum(t => (decimal?)t.Amount) ?? 0;
            ViewBag.DailyProfitAudit = dailyProfitAudit;
            ViewBag.AuditStartDate = startDate;
            ViewBag.AuditEndDate = endDate;
            ViewBag.AuditPage = currentPage;
            ViewBag.AuditTotalPages = totalPages;
            ViewBag.AuditPageSize = pageSize;
            ViewBag.AuditPageTotal = pageTotal;
            ViewBag.AuditFilteredTotal = filteredTotal;
            ViewBag.WithdrawalRequests = withdrawalRequests;
            ViewBag.DepositRequests = depositRequests;
            ViewBag.TransferRequests = transferRequests;
            ViewBag.AdminAuditLogs = auditLogs;

            ViewBag.UsersPage = usersCurrentPage;
            ViewBag.UsersTotalPages = usersTotalPages;
            ViewBag.UsersPageSize = usersPageSize;
            ViewBag.DepositsPage = depositCurrentPage;
            ViewBag.DepositsTotalPages = depositTotalPages;
            ViewBag.DepositsPageSize = depositsPageSize;
            ViewBag.TransfersPage = transferCurrentPage;
            ViewBag.TransfersTotalPages = transferTotalPages;
            ViewBag.TransfersPageSize = transfersPageSize;
            ViewBag.WithdrawalsPage = withdrawalCurrentPage;
            ViewBag.WithdrawalsTotalPages = withdrawalTotalPages;
            ViewBag.WithdrawalsPageSize = withdrawalsPageSize;

            return View(users);
        }

        [HttpPost]
        public IActionResult UpdateDepositAddress(string username, string depositAddress)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            user.DepositAddress = depositAddress?.Trim() ?? string.Empty;
            _context.SaveChanges();

            TempData["Success"] = "Deposit address updated.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateUserBalance(string username, decimal balance)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            if (balance < 0)
            {
                TempData["Error"] = "Balance cannot be negative.";
                return RedirectToAction("Index");
            }

            var delta = balance - user.Balance;
            user.Balance = balance;
            user.UpdateTier();

            if (delta != 0)
            {
                _context.Transactions.Add(new Models.Transaction
                {
                    Username = user.Username,
                    Amount = delta,
                    Type = "Admin Adjustment",
                    CreatedAt = DateTime.Now
                });
            }

            _context.AdminAuditLogs.Add(new Models.AdminAuditLog
            {
                Action = "Update balance",
                Target = $"{user.Username} -> ${balance:F2}",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();
            TempData["Success"] = "Balance updated.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ApproveWithdrawal(int requestId)
        {
            var request = _context.WithdrawalRequests.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                TempData["Error"] = "Withdrawal request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "Withdrawal request already processed.";
                return RedirectToAction("Index");
            }

            request.Status = "Approved";
            request.ApprovalCode = GenerateApprovalCode();
            request.ApprovedAt = DateTime.UtcNow;
            _context.SaveChanges();

            TempData["Success"] = "Withdrawal approved. Provide the code to the user.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RejectWithdrawal(int requestId, string reason)
        {
            var request = _context.WithdrawalRequests.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                TempData["Error"] = "Withdrawal request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "Withdrawal request already processed.";
                return RedirectToAction("Index");
            }

            request.Status = "Rejected";
            request.RejectedAt = DateTime.UtcNow;
            request.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Not specified" : reason.Trim();
            _context.SaveChanges();

            TempData["Success"] = "Withdrawal rejected.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleApprovalCodeVisibility(int requestId, bool reveal)
        {
            var request = _context.WithdrawalRequests.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                TempData["Error"] = "Withdrawal request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Approved" || string.IsNullOrWhiteSpace(request.ApprovalCode))
            {
                TempData["Error"] = "Approval code is not available yet.";
                return RedirectToAction("Index");
            }

            request.ApprovalCodeRevealed = reveal;
            request.ApprovalCodeRevealedAt = reveal ? DateTime.UtcNow : null;

            _context.AdminAuditLogs.Add(new Models.AdminAuditLog
            {
                Action = reveal ? "Reveal approval code" : "Hide approval code",
                Target = $"Withdrawal #{request.Id} ({request.Username})",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            TempData["Success"] = reveal
                ? "Approval code revealed to user."
                : "Approval code hidden from user.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ApproveDeposit(int requestId)
        {
            var request = _context.DepositRequests.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                TempData["Error"] = "Deposit request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "Deposit request already processed.";
                return RedirectToAction("Index");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            user.Balance += request.Amount;
            user.UpdateTier();

            var transaction = new Models.Transaction
            {
                Username = user.Username,
                Amount = request.Amount,
                Type = "Deposit",
                CreatedAt = DateTime.Now
            };

            request.Status = "Approved";
            request.ApprovedAt = DateTime.UtcNow;

            _context.Transactions.Add(transaction);
            _context.AdminAuditLogs.Add(new Models.AdminAuditLog
            {
                Action = "Approve deposit",
                Target = $"Deposit #{request.Id} ({request.Username})",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            TempData["Success"] = "Deposit approved.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RejectDeposit(int requestId, string reason)
        {
            var request = _context.DepositRequests.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                TempData["Error"] = "Deposit request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "Deposit request already processed.";
                return RedirectToAction("Index");
            }

            request.Status = "Rejected";
            request.RejectedAt = DateTime.UtcNow;
            request.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Not specified" : reason.Trim();

            _context.AdminAuditLogs.Add(new Models.AdminAuditLog
            {
                Action = "Reject deposit",
                Target = $"Deposit #{request.Id} ({request.Username})",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            TempData["Success"] = "Deposit rejected.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ApproveTransfer(int requestId)
        {
            var request = _context.TransferRequests.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                TempData["Error"] = "Transfer request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "Transfer request already processed.";
                return RedirectToAction("Index");
            }

            if (!request.RecipientConfirmed)
            {
                TempData["Error"] = "Recipient has not confirmed this transfer yet.";
                return RedirectToAction("Index");
            }

            var sender = _context.Users.FirstOrDefault(u => u.Username == request.SenderUsername);
            var recipient = _context.Users.FirstOrDefault(u => u.Username == request.RecipientUsername);
            if (sender == null || recipient == null)
            {
                TempData["Error"] = "Sender or recipient not found.";
                return RedirectToAction("Index");
            }

            var totalDebit = request.Amount + request.FeeAmount;
            if (sender.Balance < totalDebit)
            {
                TempData["Error"] = "Sender has insufficient balance.";
                return RedirectToAction("Index");
            }

            sender.Balance -= totalDebit;
            recipient.Balance += request.Amount;
            sender.UpdateTier();
            recipient.UpdateTier();

            request.Status = "Approved";
            request.ApprovedAt = DateTime.UtcNow;

            _context.Transactions.Add(new Models.Transaction
            {
                Username = sender.Username,
                Amount = -totalDebit,
                Type = "Transfer Out",
                CreatedAt = DateTime.Now
            });

            _context.Transactions.Add(new Models.Transaction
            {
                Username = recipient.Username,
                Amount = request.Amount,
                Type = "Transfer In",
                CreatedAt = DateTime.Now
            });

            _context.AdminAuditLogs.Add(new Models.AdminAuditLog
            {
                Action = "Approve transfer",
                Target = $"Transfer #{request.Id} ({sender.Username} -> {recipient.Username})",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            TempData["Success"] = "Transfer approved.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RejectTransfer(int requestId, string reason)
        {
            var request = _context.TransferRequests.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                TempData["Error"] = "Transfer request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Pending")
            {
                TempData["Error"] = "Transfer request already processed.";
                return RedirectToAction("Index");
            }

            request.Status = "Rejected";
            request.RejectedAt = DateTime.UtcNow;
            request.RejectionReason = string.IsNullOrWhiteSpace(reason) ? "Not specified" : reason.Trim();

            _context.AdminAuditLogs.Add(new Models.AdminAuditLog
            {
                Action = "Reject transfer",
                Target = $"Transfer #{request.Id} ({request.SenderUsername} -> {request.RecipientUsername})",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            TempData["Success"] = "Transfer rejected.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult FinalizeWithdrawal(int requestId)
        {
            var request = _context.WithdrawalRequests.FirstOrDefault(r => r.Id == requestId);
            if (request == null)
            {
                TempData["Error"] = "Withdrawal request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Processing")
            {
                TempData["Error"] = "Withdrawal is not in processing state.";
                return RedirectToAction("Index");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index");
            }

            if (request.Amount > user.Balance)
            {
                TempData["Error"] = "Insufficient balance to finalize withdrawal.";
                return RedirectToAction("Index");
            }

            user.Balance -= request.Amount;
            user.UpdateTier();

            var transaction = new Models.Transaction
            {
                Username = user.Username,
                Amount = request.Amount,
                Type = "Withdrawal",
                WalletAddress = request.WalletAddress,
                CreatedAt = DateTime.Now
            };

            request.Status = "Completed";
            request.CompletedAt = DateTime.UtcNow;
            request.FinalApprovedAt = DateTime.UtcNow;

            _context.Transactions.Add(transaction);
            _context.AdminAuditLogs.Add(new Models.AdminAuditLog
            {
                Action = "Finalize withdrawal",
                Target = $"Withdrawal #{request.Id} ({request.Username})",
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();

            TempData["Success"] = "Withdrawal finalized.";
            return RedirectToAction("Index");
        }

        private static string GenerateApprovalCode()
        {
            return Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        }

        public IActionResult ExportDailyProfitCsv(DateTime? startDate, DateTime? endDate)
        {
            var dailyProfitQuery = _context.Transactions
                .Where(t => t.Type == "Daily Profit");

            if (startDate.HasValue)
                dailyProfitQuery = dailyProfitQuery.Where(t => t.CreatedAt.Date >= startDate.Value.Date);

            if (endDate.HasValue)
                dailyProfitQuery = dailyProfitQuery.Where(t => t.CreatedAt.Date <= endDate.Value.Date);

            var records = dailyProfitQuery
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            var totalAmount = records.Sum(r => r.Amount);

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Username,Amount,Date");

            foreach (var entry in records)
            {
                var date = entry.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
                builder.AppendLine($"{entry.Username},{entry.Amount},{date}");
            }

            builder.AppendLine($"Totals,{totalAmount},");

            var fileName = "daily-profit-audit.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", fileName);
        }
    }
}
