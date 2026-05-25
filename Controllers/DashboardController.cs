using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using InvestmentWebApp.Data;
using InvestmentWebApp.Models;
using InvestmentWebApp.Hubs;
using InvestmentWebApp.Services;
using System.Linq;
using System;

namespace InvestmentWebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<DashboardHub> _hubContext;
        private readonly EmailService _emailService;

        public DashboardController(AppDbContext context, IHubContext<DashboardHub> hubContext, EmailService emailService)
        {
            _context = context;
            _hubContext = hubContext;
            _emailService = emailService;
        }

        public IActionResult Index(int page = 1, int transactionsPageSize = 10, int depositsPage = 1, int depositsPageSize = 10, int withdrawalsPage = 1, int withdrawalsPageSize = 10, int sentTransfersPage = 1, int sentTransfersPageSize = 10, int receivedTransfersPage = 1, int receivedTransfersPageSize = 10)
        {
            var username = HttpContext.Session.GetString("user");

            if (username == null)
                return RedirectToAction("Login", "Auth");

            var user = _context.Users
                .FirstOrDefault(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var nowUtc = DateTime.UtcNow;
            var cutoffTime = new TimeSpan(23, 59, 59);
            var lastEligibleDate = nowUtc.TimeOfDay >= cutoffTime
                ? nowUtc.Date
                : nowUtc.Date.AddDays(-1);

            var lastAppliedDate = user.LastProfitDate.Date;

            if (user.LastProfitDate == DateTime.MinValue)
            {
                lastAppliedDate = nowUtc.TimeOfDay >= cutoffTime
                    ? lastEligibleDate.AddDays(-1)
                    : lastEligibleDate;
            }

            var daysToApply = (lastEligibleDate - lastAppliedDate).Days;

            if (daysToApply > 0)
            {
                for (var i = 1; i <= daysToApply; i++)
                {
                    var profit = user.CalculateDailyProfit();
                    user.Balance += profit;
                    user.UpdateTier();

                    var transactionDate = lastAppliedDate
                        .AddDays(i)
                        .Add(cutoffTime);

                    var transaction = new Transaction
                    {
                        Username = user.Username,
                        Amount = profit,
                        Type = "Daily Profit",
                        CreatedAt = DateTime.SpecifyKind(transactionDate, DateTimeKind.Utc)
                    };

                    _context.Transactions.Add(transaction);
                }

                user.LastProfitDate = lastEligibleDate;
                _context.SaveChanges();
            }

            user.UpdateTier();

            _context.SaveChanges();

            var allowedPageSizes = new[] { 5, 10, 20, 50 };
            int NormalizePageSize(int value) => allowedPageSizes.Contains(value) ? value : 10;

            var normalizedTransactionsPageSize = NormalizePageSize(transactionsPageSize);
            var baseQuery = _context.Transactions
                .Where(t => t.Username == username)
                .OrderByDescending(t => t.CreatedAt);

            var totalTransactions = baseQuery.Count();
            var totalPages = (int)Math.Ceiling(totalTransactions / (double)normalizedTransactionsPageSize);
            var currentPage = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var transactions = baseQuery
                .Skip((currentPage - 1) * normalizedTransactionsPageSize)
                .Take(normalizedTransactionsPageSize)
                .ToList();

            var normalizedDepositsPageSize = NormalizePageSize(depositsPageSize);
            var normalizedWithdrawalsPageSize = NormalizePageSize(withdrawalsPageSize);
            var normalizedSentTransfersPageSize = NormalizePageSize(sentTransfersPageSize);
            var normalizedReceivedTransfersPageSize = NormalizePageSize(receivedTransfersPageSize);

            var withdrawalQuery = _context.WithdrawalRequests
                .Where(w => w.Username == username)
                .OrderByDescending(w => w.RequestedAt);
            var withdrawalTotal = withdrawalQuery.Count();
            var withdrawalTotalPages = (int)Math.Ceiling(withdrawalTotal / (double)normalizedWithdrawalsPageSize);
            var withdrawalCurrentPage = Math.Max(1, Math.Min(withdrawalsPage, withdrawalTotalPages == 0 ? 1 : withdrawalTotalPages));
            var withdrawalRequests = withdrawalQuery
                .Skip((withdrawalCurrentPage - 1) * normalizedWithdrawalsPageSize)
                .Take(normalizedWithdrawalsPageSize)
                .ToList();

            var depositQuery = _context.DepositRequests
                .Where(d => d.Username == username)
                .OrderByDescending(d => d.RequestedAt);
            var depositTotal = depositQuery.Count();
            var depositTotalPages = (int)Math.Ceiling(depositTotal / (double)normalizedDepositsPageSize);
            var depositCurrentPage = Math.Max(1, Math.Min(depositsPage, depositTotalPages == 0 ? 1 : depositTotalPages));
            var depositRequests = depositQuery
                .Skip((depositCurrentPage - 1) * normalizedDepositsPageSize)
                .Take(normalizedDepositsPageSize)
                .ToList();

            var sentTransferQuery = _context.TransferRequests
                .Where(t => t.SenderUsername == username)
                .OrderByDescending(t => t.RequestedAt);
            var sentTransferTotal = sentTransferQuery.Count();
            var sentTransferTotalPages = (int)Math.Ceiling(sentTransferTotal / (double)normalizedSentTransfersPageSize);
            var sentTransferCurrentPage = Math.Max(1, Math.Min(sentTransfersPage, sentTransferTotalPages == 0 ? 1 : sentTransferTotalPages));
            var sentTransfers = sentTransferQuery
                .Skip((sentTransferCurrentPage - 1) * normalizedSentTransfersPageSize)
                .Take(normalizedSentTransfersPageSize)
                .ToList();

            var receivedTransferQuery = _context.TransferRequests
                .Where(t => t.RecipientUsername == username)
                .OrderByDescending(t => t.RequestedAt);
            var receivedTransferTotal = receivedTransferQuery.Count();
            var receivedTransferTotalPages = (int)Math.Ceiling(receivedTransferTotal / (double)normalizedReceivedTransfersPageSize);
            var receivedTransferCurrentPage = Math.Max(1, Math.Min(receivedTransfersPage, receivedTransferTotalPages == 0 ? 1 : receivedTransferTotalPages));
            var receivedTransfers = receivedTransferQuery
                .Skip((receivedTransferCurrentPage - 1) * normalizedReceivedTransfersPageSize)
                .Take(normalizedReceivedTransfersPageSize)
                .ToList();

            var chartLabels = transactions
                .Select(t => t.CreatedAt.ToString("dd/MM"))
                .ToList();

            var chartData = transactions
                .Select(t => t.Amount)
                .ToList();

            var runningTotal = 0m;
            var balanceData = transactions
                .Select(t => runningTotal += t.Amount)
                .ToList();

            var model = new DashboardViewModel
            {
                User = user,
                Transactions = transactions,
                WithdrawalRequests = withdrawalRequests,
                DepositRequests = depositRequests,
                SentTransfers = sentTransfers,
                ReceivedTransfers = receivedTransfers,
                ChartLabels = chartLabels,
                ChartData = chartData,
                BalanceData = balanceData,
                DailyReturn = user.GetDailyReturn(),
                LastProfitDate = user.LastProfitDate,
                TransactionPage = currentPage,
                TransactionTotalPages = totalPages
            };

            ViewBag.DepositsPage = depositCurrentPage;
            ViewBag.DepositsTotalPages = depositTotalPages;
            ViewBag.WithdrawalsPage = withdrawalCurrentPage;
            ViewBag.WithdrawalsTotalPages = withdrawalTotalPages;
            ViewBag.SentTransfersPage = sentTransferCurrentPage;
            ViewBag.SentTransfersTotalPages = sentTransferTotalPages;
            ViewBag.ReceivedTransfersPage = receivedTransferCurrentPage;
            ViewBag.ReceivedTransfersTotalPages = receivedTransferTotalPages;
            ViewBag.TransactionsPageSize = normalizedTransactionsPageSize;
            ViewBag.DepositsPageSize = normalizedDepositsPageSize;
            ViewBag.WithdrawalsPageSize = normalizedWithdrawalsPageSize;
            ViewBag.SentTransfersPageSize = normalizedSentTransfersPageSize;
            ViewBag.ReceivedTransfersPageSize = normalizedReceivedTransfersPageSize;

            return View(model);
        }

        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("user");

            if (username == null)
                return RedirectToAction("Login", "Auth");

            var user = _context.Users
                .FirstOrDefault(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            user.UpdateTier();

            _context.SaveChanges();

            ViewBag.DailyReturn = user.GetDailyReturn();

            return View(user);
        }

        [HttpPost]
        public IActionResult SubmitDeposit(decimal amount)
        {
            var username = HttpContext.Session.GetString("user");

            if (username == null)
                return RedirectToAction("Login", "Auth");

            var user = _context.Users
                .FirstOrDefault(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            if (amount <= 0)
            {
                TempData["Error"] = "Please enter a valid investment amount.";
                return RedirectToAction("Index");
            }

            if (amount < 1000)
            {
                TempData["Error"] = "Minimum deposit is $1,000.00.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(user.DepositAddress))
            {
                TempData["Error"] = "Deposit address is not available yet. Please contact support.";
                return RedirectToAction("Index");
            }

            var depositRequest = new DepositRequest
            {
                Username = user.Username,
                Amount = amount,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _context.DepositRequests.Add(depositRequest);
            _context.SaveChanges();

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                _emailService.SendEmail(
                    user.Email,
                    "Deposit Request Received",
                    $"Hi {user.Username},\n\nWe received your deposit request for ${amount:N2}. Your request is now pending admin approval.\n\nThank you for investing with us.");
            }

            _ = _hubContext.Clients.All.SendAsync(
                "ReceiveBalanceUpdate",
                user.Username,
                user.Balance,
                user.GetDailyReturn(),
                user.Tier);

            TempData["Success"] = "Deposit request submitted. Waiting for admin approval.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Withdraw(decimal amount, string walletAddress)
        {
            var username = HttpContext.Session.GetString("user");

            if (username == null)
                return RedirectToAction("Login", "Auth");

            var user = _context.Users
                .FirstOrDefault(u => u.Username == username);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            if (amount <= 0)
            {
                TempData["Error"] = "Please enter a valid withdrawal amount.";
                return RedirectToAction("Index");
            }

            if (amount < 1000)
            {
                TempData["Error"] = "Minimum withdrawal is $1,000.00.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(walletAddress))
            {
                TempData["Error"] = "Please enter a USDT wallet address.";
                return RedirectToAction("Index");
            }

            if (amount > user.Balance)
            {
                TempData["Error"] = "Insufficient balance.";
                return RedirectToAction("Index");
            }

            var withdrawalRequest = new WithdrawalRequest
            {
                Username = user.Username,
                Amount = amount,
                WalletAddress = walletAddress.Trim(),
                Status = "Pending",
                RequestedAt = DateTime.UtcNow
            };

            _context.WithdrawalRequests.Add(withdrawalRequest);
            _context.SaveChanges();

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                _emailService.SendEmail(
                    user.Email,
                    "Withdrawal Request Submitted",
                    $"Hi {user.Username},\n\nYour withdrawal request for ${amount:N2} has been received and is pending approval.\n\nWe will notify you once it is processed.");
            }

            _ = _hubContext.Clients.All.SendAsync(
                "ReceiveBalanceUpdate",
                user.Username,
                user.Balance,
                user.GetDailyReturn(),
                user.Tier);

            TempData["Success"] = "Withdrawal request submitted for approval.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SubmitTransfer(decimal amount, string recipientUsername, string note)
        {
            var username = HttpContext.Session.GetString("user");

            if (username == null)
                return RedirectToAction("Login", "Auth");

            var sender = _context.Users.FirstOrDefault(u => u.Username == username);
            if (sender == null)
                return RedirectToAction("Login", "Auth");

            var normalizedRecipient = recipientUsername?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedRecipient))
            {
                TempData["Error"] = "Recipient username is required.";
                return RedirectToAction("Index");
            }

            if (normalizedRecipient == sender.Username)
            {
                TempData["Error"] = "You cannot transfer to yourself.";
                return RedirectToAction("Index");
            }

            if (amount < 100 || amount > 300000)
            {
                TempData["Error"] = "Transfer amount must be between $100 and $300,000.";
                return RedirectToAction("Index");
            }

            var todayLocal = DateTime.Now.Date;
            var todayTotal = _context.TransferRequests
                .Where(t => t.SenderUsername == sender.Username && t.RequestedAt.Date == todayLocal && t.Status != "Rejected")
                .Sum(t => t.Amount);

            if (todayTotal + amount > 300000)
            {
                TempData["Error"] = "Daily transfer limit of $300,000 reached.";
                return RedirectToAction("Index");
            }

            var recipient = _context.Users.FirstOrDefault(u => u.Username == normalizedRecipient);
            if (recipient == null)
            {
                TempData["Error"] = "Recipient not found.";
                return RedirectToAction("Index");
            }

            var feeAmount = Math.Round(amount * 0.001m, 2);
            if (sender.Balance < amount + feeAmount)
            {
                TempData["Error"] = "Insufficient balance for transfer and fee.";
                return RedirectToAction("Index");
            }

            var request = new TransferRequest
            {
                SenderUsername = sender.Username,
                RecipientUsername = recipient.Username,
                TransferCode = GenerateTransferCode(),
                Amount = amount,
                FeeAmount = feeAmount,
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
                RecipientConfirmed = false,
                Status = "Pending",
                RequestedAt = DateTime.Now
            };

            _context.TransferRequests.Add(request);
            _context.SaveChanges();

            TempData["Success"] = "Transfer submitted for admin approval.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ConfirmTransferReceipt(int requestId)
        {
            var username = HttpContext.Session.GetString("user");

            if (username == null)
                return RedirectToAction("Login", "Auth");

            var request = _context.TransferRequests
                .FirstOrDefault(t => t.Id == requestId && t.RecipientUsername == username);

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

            if (request.RecipientConfirmed)
            {
                TempData["Error"] = "Transfer already confirmed.";
                return RedirectToAction("Index");
            }

            request.RecipientConfirmed = true;
            request.RecipientConfirmedAt = DateTime.UtcNow;
            _context.SaveChanges();

            TempData["Success"] = "Transfer confirmed. Awaiting admin approval.";
            return RedirectToAction("Index");
        }

        private static string GenerateTransferCode()
        {
            return "TR-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        }

        [HttpPost]
        public IActionResult CompleteWithdrawal(int requestId, string approvalCode)
        {
            var username = HttpContext.Session.GetString("user");

            if (username == null)
                return RedirectToAction("Login", "Auth");

            var request = _context.WithdrawalRequests
                .FirstOrDefault(r => r.Id == requestId && r.Username == username);

            if (request == null)
            {
                TempData["Error"] = "Withdrawal request not found.";
                return RedirectToAction("Index");
            }

            if (request.Status != "Approved")
            {
                TempData["Error"] = "Withdrawal request is not approved yet.";
                return RedirectToAction("Index");
            }

            if (!string.Equals(request.ApprovalCode, approvalCode?.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Invalid approval code.";
                return RedirectToAction("Index");
            }

            request.Status = "Processing";
            request.ProcessingAt = DateTime.UtcNow;
            _context.SaveChanges();

            TempData["Success"] = "Withdrawal is processing. It can take between 5 minutes and 24 hours.";
            return RedirectToAction("Index");
        }

        private static string GenerateDepositAddress()
        {
            return "USDT-" + Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
        }
    }
}
