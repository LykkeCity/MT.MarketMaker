namespace MarginTrading.MarketMaker.Contracts.Models
{
    public class IsAliveResponse
    {
        public string Version { get; set; }
        public string Env { get; set; }
        public bool IsDebug { get; set; }
        public int AssetPairsCount { get; set; }
    }
}