namespace Integrations.Models.Armtek;

public class RgTab
{
    public string Kunnr { get; set; } = null!;
    public string Default { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public List<WeTab> WeTabs { get; set; } = null!;
}