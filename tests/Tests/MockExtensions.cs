using System.Threading.Tasks;
using Moq.Language.Flow;

namespace Tests
{
    internal static class MockExtensions
    {
        public static IReturnsResult<TMock> ReturnsAsync<TMock, TResult>(this ISetup<TMock, Task<TResult>> src, TResult value) where TMock : class
        {
            return src.Returns(Task.FromResult(value));
        }
    }
}
