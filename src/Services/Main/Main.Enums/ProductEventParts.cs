namespace Main.Application.Interfaces.Products;

[Flags]
public enum ProductEventParts
{
	Core = 1,
	Characteristics = 2,
	Dimensions = 4,
	Weight = 8,
	All = Core | Characteristics | Dimensions | Weight
}
