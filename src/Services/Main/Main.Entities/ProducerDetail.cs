namespace Main.Entities;

public partial class ProducerDetail
{
    public int Id { get; set; }

    public int ProducerId { get; set; }

    public string AddressType { get; set; } = null!;

    public string? Name { get; set; }

    public string? Name2 { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? CountryCode { get; set; }

    public string? Street { get; set; }

    public string? Street2 { get; set; }

    public string? PostalCountryCode { get; set; }

    public string? Phone { get; set; }

    public virtual Producer Producer { get; set; } = null!;
}
