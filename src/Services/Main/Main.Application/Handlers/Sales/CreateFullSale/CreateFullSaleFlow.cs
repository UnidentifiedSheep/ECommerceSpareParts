using Application.Common.Abstractions;
using Application.Common.Attributes;
using Main.Application.Handlers.ArticleReservations.GetArticlesWithNotEnoughStock;
using Main.Application.Handlers.ArticleReservations.SubtractCountFromReservations;
using Main.Application.Handlers.Balance.CreateTransaction;
using Main.Application.Handlers.Sales.CreateSale;
using Main.Application.Handlers.StorageContents.RemoveContent;

namespace Main.Application.Handlers.Sales.CreateFullSale;

[FlowStep<GetArticlesWithNotEnoughStockQuery>]
[FlowStep<CreateTransactionCommand>]
[FlowStep<RemoveContentCommand>]
[FlowStep<CreateSaleCommand>]
[FlowStep<CreateTransactionCommand>]
[FlowStep<SubtractCountFromReservationsCommand>]
public class CreateFullSaleFlow : CommandFlow<CreateFullSaleCommand> { }