using Enums.Units;

namespace Extensions;

public static class DimensionExtensions
{
	public static decimal ToCubicMeters(
		decimal length,
		decimal width,
		decimal height,
		DimensionUnit unit) => length.ToMeters(unit) * width.ToMeters(unit) * height.ToMeters(unit);

	public static decimal ToCubicMeters(
		decimal length,
		decimal width,
		decimal height) => length * width * height;

	public static decimal ToMeters(this decimal value, DimensionUnit unit) => value * unit.GetMetersScale();

	public static decimal GetMetersScale(this DimensionUnit unit) =>
		unit switch
		{
			DimensionUnit.Millimeter => 0.001m,
			DimensionUnit.Centimeter => 0.01m,
			DimensionUnit.Meter => 1m,
			_ => throw new ArgumentOutOfRangeException(
				nameof(unit),
				unit,
				null)
		};
}
