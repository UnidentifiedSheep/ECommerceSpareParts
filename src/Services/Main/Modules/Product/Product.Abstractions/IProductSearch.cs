namespace Main.Modules.Product.Abstractions;

public interface IProductSearch
{
	IQueryable<int> MatchingIds(string term);
}
