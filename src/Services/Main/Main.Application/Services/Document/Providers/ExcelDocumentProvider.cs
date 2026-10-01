using Application.Common.Models.Options.S3;
using Main.Application.Interfaces.Services.Document;
using Main.Enums.Documents;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace Main.Application.Services.Document.Providers;

public sealed class ExcelDocumentProvider(
	IS3Service s3Storage,
	IOptions<S3BucketsOptions> bucketsOptions,
	IDocumentTemplateCache cache
	) : CacheableDocumentProvider<ExcelDocumentTemplate>(s3Storage, bucketsOptions, cache)
{
	public override DocumentType SupportedType => DocumentType.Excel;
	protected override ExcelDocumentTemplate GenDocument(MemoryStream stream) => new(stream);
}
