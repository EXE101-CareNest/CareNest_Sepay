using CareNest_SePay.Application.Interfaces.CQRS;
using CareNest_SePay.Application.Common;

namespace CareNest_SePay.Application.Features.Queries
{
    public class GetTransactionsByDateRangeQuery : IQuery<PageResult<Domain.Entities.SepayTransaction>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
