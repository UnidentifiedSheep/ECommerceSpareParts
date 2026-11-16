using System.Reflection;

namespace Main.Modules.Platform.Application;

public static class PlatformApplicationAssembly
{
	public static Assembly Value => typeof(PlatformApplicationAssembly).Assembly;
}
