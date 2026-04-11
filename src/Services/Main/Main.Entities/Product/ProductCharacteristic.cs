namespace Main.Entities;

public class ProductCharacteristic
{
    public int ProductId { get; set; }

    public string Value { get; set; } = null!;

    public string? Name { get; set; }
}