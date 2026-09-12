using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;

namespace SchemaGeneration.Tests;

internal sealed class StubContextualStringLocalizer(IReadOnlyDictionary<string, string> values)
	: IContextualLocalizer
{
	public string Get(ILocalizableMessage message)
	{
		if (values.TryGetValue(message.MessageKey, out var value))
			return value;

		throw new InvalidOperationException();
	}
	public bool TryGet(ILocalizableMessage message, [NotNullWhen(true)] out string? value)
		=> values.TryGetValue(message.MessageKey, out value);
}
