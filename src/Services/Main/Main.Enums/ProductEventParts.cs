namespace Main.Enums;

[Flags]
public enum ProductEventParts
{
	Core = 1,
	Characteristics = 2,
	Dimensions = 4,
	Weight = 8,
	Stocks = 16,
	All = Core | Characteristics | Dimensions | Weight | Stocks
}
