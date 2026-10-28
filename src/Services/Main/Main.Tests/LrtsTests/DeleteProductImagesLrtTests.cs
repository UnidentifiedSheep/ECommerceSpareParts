using System.Text.Json;
using Domain.CommonEnums;
using FluentAssertions;
using Main.Application.Lrts.DeleteProductImages;
using Microsoft.Extensions.DependencyInjection;
using Tests.Stubs;
using Tests.TestContainers.Combined;

namespace Tests.LrtsTests;

public class DeleteProductImagesLrtTests(CombinedContainerFixture fixture) : LrtIntegrationTest<DeleteProductImagesLrt>(fixture)
{
	private S3StorageServiceStub S3 => Sp.GetRequiredService<S3StorageServiceStub>();
	private static DeleteProductImagesInputState Input => new()
	{
		ProductId = 42, BucketName = "images", StorageKeys = ["products/42/first.jpg", "products/42/second.jpg"]
	};

	[Fact]
	public async Task DeletesCapturedImages_AndAllowsRepeatedExecution()
	{
		foreach (var key in Input.StorageKeys)
			S3.SetFile("images", key, "image");
		S3.SetFile("images", "products/43/keep.jpg", "other product");

		for (var attempt = 0; attempt < 2; attempt++)
		{
			var execution = await ExecuteLrt(
				JsonSerializer.Serialize(Input),
				cancellationToken: CancellationToken);
			execution.Job.Status.Should().Be(JobStatus.Succeeded, execution.Job.ErrorMessage);
		}

		foreach (var key in Input.StorageKeys)
			(await S3.DownloadFileAsync("images", key, CancellationToken))
				.IsSuccess.Should().BeFalse();

		(await S3.DownloadFileAsync("images", "products/43/keep.jpg", CancellationToken))
			.IsSuccess.Should().BeTrue();
	}

	[Fact]
	public async Task S3Failure_FailsJobAfterRetriesAndAllowsRetryOfPartiallyDeletedImages()
	{
		S3.SetFile("images", Input.StorageKeys[0], "first");
		S3.SetFile("images", Input.StorageKeys[1], "second");
		S3.FailDeletion("images", Input.StorageKeys[1]);

		var execution = await ExecuteLrt(
			inputState: JsonSerializer.Serialize(Input),
			maxAttempts: 2,
			cancellationToken: CancellationToken);

		execution.Job.Status.Should().Be(JobStatus.Failed);
		execution.Job.Attempts.Should().Be(2);
		execution.Job.ErrorMessage.Should().Contain("AccessDenied");
		(await S3.DownloadFileAsync("images", Input.StorageKeys[0], CancellationToken)).IsSuccess.Should().BeFalse();
		(await S3.DownloadFileAsync("images", Input.StorageKeys[1], CancellationToken)).IsSuccess.Should().BeTrue();

		S3.AllowDeletion("images", Input.StorageKeys[1]);
		var retry = await ExecuteLrt(JsonSerializer.Serialize(Input), cancellationToken: CancellationToken);
		retry.Job.Status.Should().Be(JobStatus.Succeeded, retry.Job.ErrorMessage);
		(await S3.DownloadFileAsync("images", Input.StorageKeys[1], CancellationToken)).IsSuccess.Should().BeFalse();
	}
}
