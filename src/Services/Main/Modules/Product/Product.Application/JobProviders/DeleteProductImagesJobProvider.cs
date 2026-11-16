using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Attributes;
using Domain.CommonEntities.Job;
using Main.Modules.Product.Application.Lrts.DeleteProductImages;

namespace Main.Modules.Product.Application.JobProviders;

[Lifetime(Lifetime.Singleton)]
public class
	DeleteProductImagesJobProvider : IJobProvider<DeleteProductImagesLrt, DeleteProductImagesInputState>
{
	public Job Create(DeleteProductImagesInputState inputState, int maxAttempts = 3)
	{
		inputState.ValidateState();
		return SingleRunJob.CreateUnique(
			$"delete:product:{inputState.ProductId}:images:{inputState.BucketName}",
			DeleteProductImagesLrt.Name,
			JsonSerializer.Serialize(inputState),
			maxAttempts);
	}
}
