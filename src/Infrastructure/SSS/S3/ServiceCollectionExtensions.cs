using Amazon.S3;
using Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using S3.Core.Interfaces;

namespace S3;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddS3(this IServiceCollection collection)
	{
		collection.AddKeyedSingleton<IAmazonS3>(
			serviceKey: Visibility.Internal,
			implementationFactory: (sp, _) =>
			{
				var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
				return new AmazonS3Client(
					options.Login, options.Password,
					CreateConfig(options.InternalUrl, options.Region, options.ForcePathStyle));
			});

		collection.AddKeyedSingleton<IAmazonS3>(
			serviceKey: Visibility.External,
			implementationFactory: (sp, _) =>
			{
				var options = sp.GetRequiredService<IOptions<S3Options>>().Value;
				return new AmazonS3Client(
					options.Login, options.Password,
					CreateConfig(options.ExternalUrl, options.Region, options.ForcePathStyle));
			});

		collection.AddSingleton<IS3Service, S3Service>();
		return collection;
	}

	private static AmazonS3Config CreateConfig(
		string serviceUrl,
		string region,
		bool forcePathStyle)
		=> new()
		{
			ServiceURL = serviceUrl,
			ForcePathStyle = forcePathStyle,
			AuthenticationRegion = region,
			UseHttp = new Uri(serviceUrl).Scheme == Uri.UriSchemeHttp
		};
}
