using System.Reflection;
using System.Text.Json.Serialization;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;

namespace Main.Application.Document;

public abstract class DocumentDefinitionBase<TRequest, TResponse, TSchema>(
	IDocumentTemplateResolver templateResolver
	) : IDocumentDefinition<TRequest, TResponse>
	where TRequest : IDocumentRequest
	where TResponse : IDocumentResponse
{
	private static readonly FieldAccessor[] Fields = typeof(TSchema)
		.GetProperties(BindingFlags.Instance | BindingFlags.Public)
		.Where(property => property.GetIndexParameters().Length == 0 &&
		                   property.GetMethod is { IsPublic: true } &&
		                   property.GetCustomAttribute<JsonPropertyNameAttribute>() is not null)
		.Select(property => new FieldAccessor(
			property.GetCustomAttribute<JsonPropertyNameAttribute>()!.Name,
			property))
		.ToArray();

	public abstract string SystemName { get; }
	public abstract string DocumentGroup { get; }
	public Type SchemaType => typeof(TSchema);

	protected abstract DocumentTypePair[] SupportedTypePairs { get; }

	protected virtual DocumentType GetTemplateType(TRequest request)
	{
		foreach (var (outputDocumentType, inputTemplateType) in SupportedTypePairs)
			if (outputDocumentType == request.DocumentType) return inputTemplateType;

		throw new NotSupportedException(
			$"Document type '{request.DocumentType}' is not supported for '{SystemName}'.");
	}

	protected async Task<IDocumentTemplate> GetTemplateAsync(
		TRequest request,
		CancellationToken token = default)
	{
		var templateType = GetTemplateType(request);
		var fullKey = GetTemplatePath(templateType);
		return await templateResolver.TryResolveAsync(fullKey, request.DocumentType, templateType, token)
			?? throw new InvalidOperationException(
				$"Unable to find template for '{SystemName}' and key '{fullKey}'");
	}

	private string GetTemplatePath(DocumentType templateType)
		=> $"{DocumentGroup}/{SystemName}{templateType.GetFileExtension()}";

	public abstract Task<TResponse> GenerateAsync(
		TRequest request,
		CancellationToken token = default);

	public async Task<IDocumentResponse> GenerateAsync(
		IDocumentRequest data,
		CancellationToken token)
	{
		ArgumentNullException.ThrowIfNull(data);

		if (data is not TRequest request)
			throw new ArgumentException(
				$"Expected '{typeof(TRequest).FullName}', " +
				$"got '{data.GetType().FullName}'.");

		return await GenerateAsync(request, token);
	}

	/// <summary>
	/// Adds all schema properties marked with JsonPropertyName to the template.
	/// </summary>
	protected void AddAllFields(IDocumentTemplate template, TSchema schemaData)
	{
		ArgumentNullException.ThrowIfNull(template);
		ArgumentNullException.ThrowIfNull(schemaData);

		foreach (var field in Fields)
			template.AddData(field.Name, field.Property.GetValue(schemaData));
	}

	private readonly record struct FieldAccessor(string Name, PropertyInfo Property);

	protected readonly record struct DocumentTypePair(
		DocumentType OutputDocumentType,
		DocumentType InputTemplateType);
}
