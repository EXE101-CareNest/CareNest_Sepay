using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Domain.Entities;

namespace CareNest_SePay.Application.Features.Queries
{
    public class GetTransactionByIdQuery : IQuery<SepayTransaction?>
    {
        public string Id { get; set; } = string.Empty;
    }
}
