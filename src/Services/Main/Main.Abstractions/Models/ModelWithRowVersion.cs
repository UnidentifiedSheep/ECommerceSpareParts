namespace Main.Abstractions.Models;

public record ModelWithRowVersion<TModel, TCode>(TModel Model, TCode RowVersion);