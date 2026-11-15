using System.Reflection;

namespace Main.Modules.Order.Application;

public static class OrderApplicationAssembly
{
	public static Assembly Value => typeof(OrderApplicationAssembly).Assembly;
}
