namespace Main.Abstractions.Models;

public record ModelWithCode<TModel, TCode>(TModel Model, TCode Code);