using System.Reflection;

namespace Main.Modules.Product.Application;

public static class ProductApplicationAssembly
{
	public static Assembly Value => typeof(ProductApplicationAssembly).Assembly;
}
