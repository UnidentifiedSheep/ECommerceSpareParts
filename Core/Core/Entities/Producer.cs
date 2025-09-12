namespace Core.Entities;

public partial class Producer
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsOe { get; set; }

    public string? ImagePath { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    public virtual ICollection<ProducerDetail> ProducerDetails { get; set; } = new List<ProducerDetail>();

    public virtual ICollection<ProducersOtherName> ProducersOtherNames { get; set; } = new List<ProducersOtherName>();
}
