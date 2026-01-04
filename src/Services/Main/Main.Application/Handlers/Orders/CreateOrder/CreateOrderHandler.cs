using Application.Common.Interfaces;
using Main.Core.Models;

namespace Main.Application.Handlers.Orders.CreateOrder;

public record CreateOrderCommand() : ICommand<CreateOrderResult>;
public record CreateOrderResult(Guid OrderId);

public class CreateOrderHandler(Settings settings) : ICommandHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}