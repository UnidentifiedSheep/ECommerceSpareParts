using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;
using Enums;
using Locan.Core.Interfaces;
using Main.Application.Document.Interfaces.Document;
using Main.Application.Document.Models;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;
using S3.Core.Options;

namespace Main.Application.Document.Document;

public abstract class DocumentDefinitionBase<TRequest, TSchema>(
	IDocumentTemplateResolver templateResolver,
	IS3Service s3Service,
	IOptions<S3BucketsOptions> options
) : IDocumentDefinition<TRequest, DocumentResponse>
	where TRequest : IDocumentRequest
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

	protected abstract DocumentTypePair[] SupportedTypePairs { get; }

	public abstract string SystemName { get; }
	public abstract string DocumentGroup { get; }
	public abstract IReadOnlyCollection<PermissionCodes> RequiredPermissions { get; }
	public abstract ILocalizableMessage Name { get; }
	public abstract ILocalizableMessage Description { get; }
	public Type SchemaType => typeof(TSchema);
	public Type RequestType => typeof(TRequest);

	public bool SupportsDocumentType(DocumentType documentType)
		=> SupportedTypePairs.Any(pair => pair.OutputDocumentType == documentType);

	public async Task<DocumentResponse> GenerateAsync(
		TRequest request,
		CancellationToken token = default)
	{
		var data = await GetSchemaDataAsync(request, token);

		await using var output = new FileStream(
			Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
			new FileStreamOptions
			{
				Mode = FileMode.CreateNew,
				Access = FileAccess.ReadWrite,
				Share = FileShare.None,
				Options = FileOptions.DeleteOnClose
			});

		var renderedType = await RenderCoreAsync(data, request.DocumentType, request.Culture, output, token);
		output.Position = 0;

		var uploadStartedAtUtc = DateTime.UtcNow;
		var key = $"Generated/{DocumentGroup}/{SystemName}/" +
			$"{uploadStartedAtUtc:yyyy/MM/dd}/{Guid.NewGuid():N}" +
			renderedType.GetFileExtension();
		var bucket = options.Value.Documents;
		var uploadedKey = await s3Service.UploadFileAsync(
			bucket.Name,
			output,
			key,
			renderedType.GetContentType());

		return new DocumentResponse
		{
			BucketName = bucket.Name,
			StorageKey = uploadedKey
		};
	}

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

	public async Task RenderAsync(
		object schemaData,
		DocumentType documentType,
		string culture,
		Stream output,
		CancellationToken token = default)
	{
		ArgumentNullException.ThrowIfNull(schemaData);

		if (schemaData is not TSchema schema)
			throw new ArgumentException(
				$"Expected '{typeof(TSchema).FullName}', " +
				$"got '{schemaData.GetType().FullName}'.");

		await RenderCoreAsync(schema, documentType, culture, output, token);
	}

	private async Task<DocumentType> RenderCoreAsync(
		TSchema schemaData,
		DocumentType documentType,
		string culture,
		Stream output,
		CancellationToken token)
	{
		using var template = await GetTemplateAsync(documentType, culture, token);
		AddAllFields(template, schemaData);
		await template.RenderAsync(output, token);
		return template.Type;
	}

	protected virtual DocumentType GetTemplateType(DocumentType documentType)
	{
		foreach (var (outputDocumentType, inputTemplateType) in SupportedTypePairs)
			if (outputDocumentType == documentType)
				return inputTemplateType;

		throw new NotSupportedException(
			$"Document type '{documentType}' is not supported for '{SystemName}'.");
	}

	protected async Task<IDocumentTemplate> GetTemplateAsync(
		DocumentType documentType,
		string cultureName,
		CancellationToken token = default)
	{
		var templateType = GetTemplateType(documentType);
		var culture = DocumentCulture.GetRequired(cultureName);

		foreach (var templateCultureName in GetTemplateCultureNames(culture))
		{
			var template = await templateResolver.TryResolveAsync(
				CreateTemplateKey(templateCultureName, templateType),
				documentType,
				token);
			if (template is not null) return template;
		}

		return await templateResolver.TryResolveAsync(
				CreateTemplateKey(null, templateType),
				documentType,
				token)
			?? throw new InvalidOperationException(
				$"Unable to find template for '{SystemName}' and culture '{culture.Name}'.");
	}

	private DocumentTemplateKey CreateTemplateKey(string? cultureName, DocumentType templateType)
		=> new(GetType(), DocumentGroup, SystemName, cultureName, templateType);

	private static IEnumerable<string> GetTemplateCultureNames(CultureInfo culture)
	{
		yield return culture.Name;
		if (!string.Equals(
				culture.Name,
				culture.TwoLetterISOLanguageName,
				StringComparison.OrdinalIgnoreCase))
			yield return culture.TwoLetterISOLanguageName;
	}

	protected abstract Task<TSchema> GetSchemaDataAsync(
		TRequest request,
		CancellationToken token);

	/// <summary>
	///     Adds all schema properties marked with JsonPropertyName to the template.
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
