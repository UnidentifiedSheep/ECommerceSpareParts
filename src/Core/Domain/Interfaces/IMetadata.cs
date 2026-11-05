namespace Domain.Interfaces;

public interface IMetadata<TMetadata> where TMetadata : class
{
	TMetadata Metadata { get; }
}
