using System.Linq.Expressions;
using Domain;
using Domain.Interfaces;
using Domain.Validation;

namespace Main.Entities.Product;

public class ProductCharacteristic : Entity<ProductCharacteristic, (int, int)>,
	ILinqEntity<ProductCharacteristic, (int, int)>, IGenerateAutomaticDomainEvents
{
	private ProductCharacteristic() {}

	private ProductCharacteristic(int productId, int characteristicId, string value)
	{
		ProductId = productId;
		CharacteristicId = characteristicId;
		SetValue(value);
	}

	public int ProductId { get; }
	public int CharacteristicId { get; }
	public Characteristic Characteristic { get; private set; } = null!;
	public string Value { get; private set; } = null!;

	public static Expression<Func<ProductCharacteristic, (int, int)>> GetKeySelector() => x =>
		ValueTuple.Create(x.ProductId, x.CharacteristicId);

	public static Expression<Func<ProductCharacteristic, bool>> GetEqualityExpression((int, int) key) =>
		x => x.ProductId == key.Item1 && x.CharacteristicId == key.Item2;

	public static ProductCharacteristic Create(int productId, int characteristicId, string value) =>
		new(productId, characteristicId, value);

	public void SetValue(string value)
	{
		Value = value
			.Trim()
			.EnsureMinLength(3, ArticleCharacteristicValueMinLengthMessage.Instance)
			.EnsureMaxLength(128, ArticleCharacteristicValueMaxLengthMessage.Instance);
	}

	public override (int, int) GetId() => (ProductId, CharacteristicId);
}
