namespace Core.Abstractions;

public abstract class BaseExceptionExample
{
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int Status { get; set; }
    public string Instance { get; set; } = string.Empty;
    public string Type { get; set; } = $"https://httpstatuses.io/";
    public string TraceId { get; set; } = "0HLV7K1111111:00000001";

    public object ErrorRelatedData { get; set; } = new
    {
        Id = 1000,
        Name = "Name",
    };

    public List<object> ValidationErrors { get; set; } =
    [
        new
        {
            PropertyName = "Name",
            ErrorMessage = "Too short",
            AttemptedValue = ""
        }
    ];
}