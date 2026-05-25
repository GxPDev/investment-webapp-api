namespace InvestmentWebApp.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
        public string Tier { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public DateTime LastProfitDate { get; set; } = DateTime.MinValue;
        public string DepositAddress { get; set; } = string.Empty;

        public decimal GetDailyReturn()
        {
            return Tier switch
            {
                "Bronze" => Balance * 0.001m,
                "Silver" => Balance * 0.003m,
                "Gold" => Balance * 0.004m,
                "VIP" => Balance * 0.005m,
                _ => 0
            };
        }

        public decimal CalculateDailyProfit()
        {
            return Tier switch
            {
                "Bronze" => Balance * 0.001m,
                "Silver" => Balance * 0.003m,
                "Gold" => Balance * 0.004m,
                "VIP" => Balance * 0.005m,
                _ => 0
            };
        }

        public void UpdateTier()
        {
            if (Balance >= 100000)
                Tier = "VIP";
            else if (Balance >= 10000)
                Tier = "Gold";
            else if (Balance >= 1000)
                Tier = "Silver";
            else
                Tier = "Bronze";
        }
    }
}
