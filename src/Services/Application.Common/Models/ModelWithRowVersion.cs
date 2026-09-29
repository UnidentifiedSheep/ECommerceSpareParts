namespace Application.Common.Models;

public record ModelWithRowVersion<TModel, TCode>(TModel Model, TCode RowVersion);
