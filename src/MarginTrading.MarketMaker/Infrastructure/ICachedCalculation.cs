namespace MarginTrading.MarketMaker.Infrastructure
{
    public interface ICachedCalculation<out TResult>
    {
        TResult Get();
    }
}