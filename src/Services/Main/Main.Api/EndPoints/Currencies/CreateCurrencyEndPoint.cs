using Carter;
using Main.Application.Handlers.Currencies.CreateCurrency;
using Mapster;
using MediatR;

namespace Main.Api.EndPoints.Currencies;

public record CreateCurrencyRequest(string ShortName, string Name, string CurrencySign, string Code);
public record CreateCurrencyResponse(int Id);

public class CreateCurrencyEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/currencies", async (ISender sender, CreateCurrencyRequest request, CancellationToken cancellationToken) =>
        {
            var command = request.Adapt<CreateCurrencyCommand>();
            var result = await sender.Send(command, cancellationToken);
            var response = new CreateCurrencyResponse(result.Id);
            return Results.Created($"currencies/{result.Id}", response);
        }).WithTags("Currencies")
        .WithDescription("Создание валюты")
        .WithDisplayName("Создание валюты");
    }
}