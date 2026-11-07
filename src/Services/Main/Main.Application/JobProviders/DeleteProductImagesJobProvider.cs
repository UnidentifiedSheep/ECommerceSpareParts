using System.Text.Json;
using Application.Common.Interfaces.Lrt;
using Attributes;
using Domain.CommonEntities.Job;
using Main.Application.Lrts.DeleteProductImages;

namespace Main.Application.JobProviders;

[Lifetime(Lifetime.Singleton)]
public class DeleteProductImagesJobProvider : IJobProvider<DeleteProductImagesLrt, DeleteProductImagesInputState>
{
	public Job Create(DeleteProductImagesInputState inputState, int maxAttempts = 3)
	{
		inputState.ValidateState();
		return SingleRunJob.CreateUnique(
			$"delete:product:{inputState.ProductId}:images:{inputState.BucketName}",
			DeleteProductImagesLrt.Name,
			initialState: JsonSerializer.Serialize(inputState),
			maxAttempts: maxAttempts);
	}
}
