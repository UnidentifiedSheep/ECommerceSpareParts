using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Main.Abstractions.Interfaces.DbRepositories;
using Main.Application.Extensions;
using Main.Application.Notifications;
using Main.Entities;
using Main.Enums;
using MediatR;

namespace Main.Application.Handlers.ArticleSizes.SetArticleSizes;

[Transactional]
public record SetArticleSizesCommand(int ArticleId, decimal Length, decimal Width, decimal Height, DimensionUnit Unit) : ICommand;

public class SetArticleSizesHandler(IArticleSizesRepository sizesRepository, IUnitOfWork unitOfWork, IMediator mediator) 
    : ICommandHandler<SetArticleSizesCommand>
{
    public async Task<Unit> Handle(SetArticleSizesCommand request, CancellationToken cancellationToken)
    {
        decimal height = request.Height;
        decimal length = request.Length;
        decimal width = request.Width;
        var sizes = await sizesRepository.GetArticleSizes(request.ArticleId, true, cancellationToken);

        if (sizes == null)
        {
            sizes = new ArticleSize();
            await unitOfWork.AddAsync(sizes, cancellationToken);
        }
        
        sizes.Unit = request.Unit;
        sizes.Height = height;
        sizes.Length = length;
        sizes.Width = width;
        sizes.VolumeM3 = DimensionExtensions.ToCubicMeters(length, width, height, request.Unit);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        
        //publish notification to invalidate cache
        await mediator.Publish(new ArticleSizeUpdatedNotification(request.ArticleId), cancellationToken);
        
        return Unit.Value;
    }
}