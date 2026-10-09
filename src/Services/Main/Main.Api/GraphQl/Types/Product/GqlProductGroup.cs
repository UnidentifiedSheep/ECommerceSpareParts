namespace Main.Api.GraphQl.Types.Product;

public record GqlProductGroup
{
	public int Id { get; }

	public GqlProductGroup(int id)
	{
		Id = id;
	}

	public GqlProductGroup()
	{

	}
}
