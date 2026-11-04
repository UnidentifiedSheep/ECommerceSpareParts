using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;

namespace Domain;

public abstract class AuditableEntity<TModel, TKey, TMetadata> : AuditableEntity<TModel, TKey>, IMetadata<TMetadata>
	where TModel : Entity<TModel, TKey>
	where TKey : notnull
	where TMetadata : class, new()
{
	[NotMapped]
	public TMetadata Metadata { get; protected set; } = new();
}
