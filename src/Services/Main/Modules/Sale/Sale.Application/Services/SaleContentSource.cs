using Application.Common.Interfaces.Repositories;
using Main.Entities.Sale.Sale;
using Main.Modules.Sale.Abstractions;
using Main.Modules.Sale.Abstractions.Interfaces;
using Main.Modules.Sale.Abstractions.Models;

namespace Main.Modules.Sale.Application.Services;

public sealed class SaleContentSource(IReadRepository<SaleContent, int> contents) : ISaleContentSource
{
	public IQueryable<SaleContentRef> Query
		=> contents.Query.Select(content => new SaleContentRef
		{
			SaleContentId = content.Id,
			ProductId = content.ProductId
		});
}
