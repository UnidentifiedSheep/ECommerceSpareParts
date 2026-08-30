using Analytics.Entities;
using Application.Common.Interfaces.Repositories;

namespace Analytics.Application.Extensions;

public static class SaleFactFilterExtensions
{
	public static IQueryable<SalesFact> ExcludeDeleted(this IQueryable<SalesFact> query) =>
		query.Where(x => !x.IsDeleted);

	public static CriteriaBuilder<SalesFact> ExcludeDeleted(this CriteriaBuilder<SalesFact> criteria) =>
		criteria.Where(x => !x.IsDeleted);

	public static IQueryable<SaleContent> ExcludeDeleted(this IQueryable<SaleContent> query) =>
		query.Where(x => !x.Sale.IsDeleted);

	public static CriteriaBuilder<SaleContent> ExcludeDeleted(this CriteriaBuilder<SaleContent> criteria) =>
		criteria.Where(x => !x.Sale.IsDeleted);

	public static IQueryable<SaleContentDetail> ExcludeDeleted(this IQueryable<SaleContentDetail> query) =>
		query.Where(x => !x.SaleContent.Sale.IsDeleted);

	public static CriteriaBuilder<SaleContentDetail> ExcludeDeleted(
		this CriteriaBuilder<SaleContentDetail> criteria) =>
		criteria.Where(x => !x.SaleContent.Sale.IsDeleted);
}
