using Analytics.Entities;
using Application.Common.Interfaces.Repositories;

namespace Analytics.Application.Interfaces.Repositories;

public interface ISalesFactRepository : IRepository<SalesFact, Guid>
{
}