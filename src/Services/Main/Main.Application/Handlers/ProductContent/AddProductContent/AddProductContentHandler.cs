using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Attributes;
using MediatR;

namespace Main.Application.Handlers.ProductContent.AddProductContent;

[AutoSave]
[Transactional]
public record AddProductContentCommand(int ParentProductId, Dictionary<int, int> Contents) : ICommand;

public class AddProductContentHandler(IUnitOfWork unitOfWork) : ICommandHandler<AddProductContentCommand>
{
    public async Task<Unit> Handle(AddProductContentCommand request, CancellationToken cancellationToken)
    {
        var contents = request.Contents
            .Select(x =>
                Entities.Product.ProductContent.Create(request.ParentProductId, x.Key, x.Value)
            ).ToList();

        await unitOfWork.AddRangeAsync(contents, cancellationToken);
        return Unit.Value;
    }
}