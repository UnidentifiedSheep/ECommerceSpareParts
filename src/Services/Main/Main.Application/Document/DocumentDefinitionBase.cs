using System.Reflection;
using System.Text.Json.Serialization;
using Application.Common.Models.Options.S3;
using Locan.Core.Interfaces;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Document;

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

	public abstract string SystemName { get; }
	public abstract string DocumentGroup { get; }
	public abstract ILocalizableMessage Name { get; }
	public abstract ILocalizableMessage Description { get; }
	public Type SchemaType => typeof(TSchema);
	public Type RequestType => typeof(TRequest);

	protected abstract DocumentTypePair[] SupportedTypePairs { get; }

	public bool SupportsDocumentType(DocumentType documentType) =>
		SupportedTypePairs.Any(pair => pair.OutputDocumentType == documentType);

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

	public async Task<DocumentResponse> GenerateAsync(
		TRequest request,
		CancellationToken token = default)
	{
		using var template = await GetTemplateAsync(request, token);
		var data = await GetSchemaDataAsync(request, token);
		AddAllFields(template, data);

		await using var output = new FileStream(
			Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()),
			new FileStreamOptions
			{
				Mode = FileMode.CreateNew,
				Access = FileAccess.ReadWrite,
				Share = FileShare.None,
				Options = FileOptions.DeleteOnClose
			});

		await template.RenderAsync(output, token);
		output.Position = 0;

		var uploadStartedAtUtc = DateTime.UtcNow;
		var key = $"Generated/{DocumentGroup}/{SystemName}/" +
		          $"{uploadStartedAtUtc:yyyy/MM/dd}/{Guid.NewGuid():N}" +
		          template.Type.GetFileExtension();
		var bucket = options.Value.Documents;
		var uploadedKey = await s3Service.UploadFileAsync(
			bucket.Name,
			output,
			key,
			template.Type.GetContentType());

		return new DocumentResponse
		{
			GeneratedFileLink = $"{bucket.PublicBaseUrl.TrimEnd('/')}/{uploadedKey}",
			BucketName = bucket.Name,
			StorageKey = uploadedKey
		};
	}

	protected abstract Task<TSchema> GetSchemaDataAsync(
		TRequest request,
		CancellationToken token);

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
