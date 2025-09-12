namespace Core.Dtos.Amw.ArticleCharacteristics;

public class NewCharacteristicsDto
{
    public int ArticleId { get; set; }
    public string? Name { get; set; }
    private string _value = string.Empty;
    public string Value 
    { 
        get => _value; 
        set => _value = value.Trim(); 
    }
    
}