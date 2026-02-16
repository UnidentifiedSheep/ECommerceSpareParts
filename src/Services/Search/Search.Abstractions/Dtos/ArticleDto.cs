using Sannr;

namespace Search.Abstractions.Dtos;

public partial class ArticleDto
{
    [Required(ErrorMessage = "Id обязателен.")]
    [Range(0, int.MaxValue, ErrorMessage = "Id артикула должен быть не отрицательным числом.")]
    public int Id { get; set; }
    [Sanitize(Trim = true, ToUpper = true)]
    [Required(ErrorMessage = "Артикул обязателен.")]
    [StringLength(128, MinimumLength = 3, ErrorMessage = "Минимальная длина артикула 3 символа, максимальная 128 символов.")]
    public string ArticleNumber { get; set; } = null!;
    [Sanitize(Trim = true)]
    [Required(ErrorMessage = "Название обязательно.")]
    [StringLength(256, MinimumLength = 3, ErrorMessage = "Минимальная длина названия 3 символа, максимальная 256 символов.")]
    public string Title { get; set; } = null!;
    [Required(ErrorMessage = "Id производителя обязателен.")]
    [Range(0, int.MaxValue, ErrorMessage = "Id производителя должно быть неотрицательным числом.")]
    public int ProducerId { get; set; }
    [Sanitize(Trim = true)]
    [Required(ErrorMessage = "Название производителя обязательно.")]
    [StringLength(128, MinimumLength = 3, ErrorMessage = "Минимальная длина названия производителя 3 символа, максимальная 128 символов.")]
    public string ProducerName { get; set; } = null!;
    public long Popularity { get; set; }


    public static async Task<ValidationResult> ValidateAsync(ArticleDto model)
    {
        return await ArticleDtoValidator.ValidateAsync(new SannrValidationContext(model));
    }
}