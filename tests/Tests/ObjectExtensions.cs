using System.Threading.Tasks;
using Microsoft.Rest;

namespace Tests
{
    internal static class ObjectExtensions
    {
        public static Task<T> ToTask<T>(this T obj)
        {
            return Task.FromResult(obj);
        }

        public static Task<HttpOperationResponse<T>> ToResponse<T>(this T obj)
        {
            return Task.FromResult(new HttpOperationResponse<T> {Body = obj});
        }
    }
}
