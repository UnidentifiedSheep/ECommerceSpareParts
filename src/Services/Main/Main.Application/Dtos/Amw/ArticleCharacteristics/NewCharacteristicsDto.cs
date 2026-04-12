namespace Main.Abstractions.Dtos.Amw.ArticleCharacteristics;

public class NewCharacteristicsDto
{
    private string _value = string.Empty;
    public int ArticleId { get; set; }
    public string? Name { get; set; }

    public string Value
    {
        get => _value;
        set => _value = value.Trim();
    }
}