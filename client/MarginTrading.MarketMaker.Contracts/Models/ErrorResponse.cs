using JetBrains.Annotations;

namespace MarginTrading.MarketMaker.Contracts.Models
{
    [UsedImplicitly] // from startup.cs only in release configuration
    public class ErrorResponse
    {
        [UsedImplicitly]
        public string ErrorMessage { get; set; }
    }
}