using Application.Common.Interfaces;
using Core.Attributes;
using Core.Interfaces.Services;
using Exceptions.Exceptions.Articles;
using Main.Application.Extensions;
using Main.Application.Validation;
using Main.Core.Abstractions;
using Main.Core.Dtos.Amw.ArticleCharacteristics;
using Main.Core.Entities;
using Mapster;

namespace Main.Application.Handlers.ArticleCharacteristics.AddCharacteristics;

[Transactional]
public record AddCharacteristicsCommand(IEnumerable<NewCharacteristicsDto> Characteristics)
    : ICommand<AddCharacteristicsResult>;

public record AddCharacteristicsResult(IEnumerable<int> Ids);

public class AddCharacteristicsHandler(IUnitOfWork unitOfWork, DbDataValidatorBase dbValidator) : ICommandHandler<AddCharacteristicsCommand, AddCharacteristicsResult>
{
    public async Task<AddCharacteristicsResult> Handle(AddCharacteristicsCommand request, CancellationToken cancellationToken)
    {
        var adapted = request.Characteristics.Adapt<List<ArticleCharacteristic>>();
        await ValidateData(adapted, cancellationToken);
        await unitOfWork.AddRangeAsync(adapted, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new (adapted.Select(x => x.Id));
    }

    private async Task ValidateData(List<ArticleCharacteristic> articles, CancellationToken ct = default)
    {
        var plan = new ValidationPlan()
            .EnsureArticleExists(articles.Select(x => x.ArticleId));
        
        await dbValidator.Validate(plan, true, true, ct);
    }
}