using System.Text.Json.Serialization;
using Api.Common.Extensions;
using Api.Common.Models.Requests;
using Enums;
using Main.Application.Dtos.Product.Reservation;
using Main.Application.Handlers.ProductReservations.CreateProductReservation;
using Main.Application.Handlers.ProductReservations.DeleteProductReservation;
using Main.Application.Handlers.ProductReservations.EditProductReservation;
using Main.Application.Handlers.ProductReservations.GetProductReservations;
using Main.Application.Handlers.ProductReservations.GetReservationHistory;
using Main.Entities.Event;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Main.Api.EndPoints.Products;

public record CreateProductReservationRequest
{
    [JsonPropertyName("reservation")]
    public required NewProductReservationDto Reservation { get; init; }
}

public record CreateProductReservationResponse
{
    [JsonPropertyName("reservation")]
    public required ProductReservationDto Reservation { get; init; }
}

public record EditProductReservationRequest
{
    [JsonPropertyName("newValue")]
    public required EditProductReservationDto NewValue { get; init; }
}

public record GetProductReservationsRequest : SortablePaginationQueryModel
{
    [FromQuery(Name = "productId")]
    public int? ProductId { get; init; }

    [FromQuery(Name = "userId")]
    public Guid? UserId { get; init; }

    [FromQuery(Name = "showDeleted")]
    public bool ShowDeleted { get; init; }
}

public record GetProductReservationsResponse
{
    [JsonPropertyName("reservations")]
    public required IReadOnlyList<ProductReservationDto> Reservations { get; init; }
}

public record GetProductReservationHistoryResponse
{
    [JsonPropertyName("history")]
    public required IReadOnlyList<ReservationManualChangeEventData> History { get; init; }
}

public static class ProductReservationsEndPoints
{
    public static RouteGroupBuilder MapProductReservationsEndPoints(this RouteGroupBuilder products)
    {
        products.MapPost(
                "/reservations",
                async (
                    ISender sender,
                    CreateProductReservationRequest request,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new CreateProductReservationCommand(request.Reservation),
                        cancellationToken);
                    return Results.Created(
                        $"/products/reservations/{result.Reservation.Id}",
                        new CreateProductReservationResponse
                        {
                            Reservation = result.Reservation
                        });
                })
            .WithName("CreateProductReservations")
            .WithSummary("Создать резервации продуктов")
            .WithDisplayName("Создать резервацию")
            .WithDescription("Создать резервацию для пользователя")
            .Accepts<CreateProductReservationRequest>(false, "application/json")
            .Produces<CreateProductReservationResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_CREATE);

        products.MapPut(
                "/reservations/{reservationId:int}",
                async (
                    ISender sender,
                    int reservationId,
                    EditProductReservationRequest request,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(
                        new EditProductReservationCommand(reservationId, request.NewValue),
                        cancellationToken);
                    return Results.NoContent();
                })
            .WithName("EditProductReservation")
            .WithSummary("Редактировать резервацию продукта")
            .WithDisplayName("Редактирование резервации")
            .WithDescription("Редактирование резервации")
            .Accepts<EditProductReservationRequest>(false, "application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_EDIT);

        products.MapDelete(
                "/reservations/{reservationId:int}",
                async (
                    ISender sender,
                    int reservationId,
                    CancellationToken cancellationToken) =>
                {
                    await sender.Send(new DeleteProductReservationCommand(reservationId), cancellationToken);
                    return Results.NoContent();
                })
            .WithName("DeleteProductReservation")
            .WithSummary("Удалить резервацию продукта")
            .WithDisplayName("Удалить резервацию")
            .WithDescription("Удалить резервацию")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_DELETE);

        products.MapGet(
                "/reservations",
                async (
                    ISender sender,
                    [AsParameters] GetProductReservationsRequest queryParams,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetProductReservationsQuery(
                        queryParams.ProductId,
                        queryParams.UserId,
                        queryParams.SortBy,
                        queryParams.ShowDeleted,
                        queryParams);
                    var result = await sender.Send(query, cancellationToken);
                    var response = new GetProductReservationsResponse
                    {
                        Reservations = result.Reservations
                    };
                    return Results.Ok(response);
                })
            .WithName("GetProductReservations")
            .WithSummary("Получить резервации продукта")
            .WithDescription("Получить список резерваций артикула по id артикула.")
            .WithDisplayName("Получение резерваций артикула")
            .Produces<GetProductReservationsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_GET_ALL);

        products.MapGet(
                "/{productId:int}/reservations",
                async (
                    ISender sender,
                    int productId,
                    [AsParameters] GetProductReservationsRequest queryParams,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetProductReservationsQuery(
                        productId,
                        queryParams.UserId,
                        queryParams.SortBy,
                        queryParams.ShowDeleted,
                        queryParams);
                    var result = await sender.Send(query, cancellationToken);
                    var response = new GetProductReservationsResponse
                    {
                        Reservations = result.Reservations
                    };
                    return Results.Ok(response);
                })
            .WithName("GetProductReservationsByProduct")
            .WithSummary("Получить резервации продукта")
            .WithDescription("Получить список резерваций артикула по id артикула.")
            .WithDisplayName("Получение резерваций артикула")
            .Produces<GetProductReservationsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_GET_ALL);

        products.MapGet(
                "/reservations/{id:int}/history",
                async (
                    ISender sender,
                    int id,
                    [AsParameters] PaginationQueryModel queryParams,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetReservationHistoryQuery(
                        id,
                        queryParams);
                    var result = await sender.Send(query, cancellationToken);
                    var response = new GetProductReservationHistoryResponse
                    {
                        History = result.History
                    };
                    return Results.Ok(response);
                })
            .WithName("GetProductReservationsHistory")
            .WithSummary("Получить историю резервации")
            .WithDescription("Получить список истории резервации.")
            .WithDisplayName("Получение истории резерваций")
            .Produces<GetProductReservationHistoryResponse>()
            .RequireAnyPermission(PermissionCodes.ARTICLE_RESERVATIONS_GET_ALL);

        return products;
    }
}