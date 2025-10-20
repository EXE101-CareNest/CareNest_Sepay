using CareNest_SePay.Application.Common;

namespace CareNest_SePay.Application.Interfaces.Services
{
    public interface IAPIService
    {
        Task<ResponseResult<T>> GetAsync<T>(string serviceType, string endpoint);
        Task<ResponseResult<T>> PostAsync<T>(string serviceType, string endpoint, object data);
        Task<ResponseResult<T>> PutAsync<T>(string serviceType, string endpoint, object data);
        Task<ResponseResult<T>> DeleteAsync<T>(string serviceType, string endpoint);
    }
}
