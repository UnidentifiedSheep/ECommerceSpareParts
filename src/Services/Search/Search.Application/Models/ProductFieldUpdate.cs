using System.Linq.Expressions;
using System.Runtime.Serialization;
using Search.Entities;

namespace Search.Application.Models;

public abstract record ProductFieldUpdate
{
	[IgnoreDataMember]
	public required int ProductId { get; init; }

	[IgnoreDataMember]
	public DateTime UpdatedAt { get; init; }

	public abstract Expression<Func<Product, DateTime?>> GetUpdatedAtField();
}
