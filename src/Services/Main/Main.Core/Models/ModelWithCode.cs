namespace Main.Core.Models;

public record ModelWithCode<TModel, TCode>(TModel Model, TCode Code);