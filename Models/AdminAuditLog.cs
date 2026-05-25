using System;

namespace InvestmentWebApp.Models
{
    public class AdminAuditLog
    {
        public int Id { get; set; }
        public string Actor { get; set; } = "Admin";
        public string Action { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
