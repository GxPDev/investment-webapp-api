namespace InvestmentWebApp.Models
{
    public class User
    {
        public string Name { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public decimal Balance { get; set; }

        public void UpdateTier()
        {
            if (Balance >= 20000m)
                Tier = "VIP";
            else if (Balance >= 5000m)
                Tier = "Gold";
            else if (Balance >= 1000m)
                Tier = "Silver";
            else
                Tier = "Bronze";
        }

        public decimal GetDailyReturn()
        {
            return Tier switch
            {
                "Bronze" => Balance * 0.01m,
                "Silver" => Balance * 0.02m,
                "Gold" => Balance * 0.05m,
                "VIP" => Balance * 0.10m,
                _ => 0
            };
        }
    }
}
