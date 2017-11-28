using JetBrains.Annotations;

namespace MarginTrading.MarketMaker.Models.Api
{
    [UsedImplicitly] // from startup.cs only in release configuration
    public class ErrorResponse
    {
        [UsedImplicitly]
        public string ErrorMessage { get; set; }
    }
}