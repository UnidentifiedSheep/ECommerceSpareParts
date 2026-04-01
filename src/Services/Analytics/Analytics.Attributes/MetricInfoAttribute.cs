namespace Analytics.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class MetricInfoAttribute : Attribute
{
    public MetricInfoAttribute(string systemName, 
        string nameLocalizationKey, 
        string descriptionLocalizationKey)
    {
        SystemName = systemName;
        NameLocalizationKey = nameLocalizationKey;
        DescriptionLocalizationKey = descriptionLocalizationKey;
    }

    /// <summary>
    /// This constructor sets all fields as system name.
    /// </summary>
    /// <param name="systemName"></param>
    public MetricInfoAttribute(string systemName)
    {
        SystemName = systemName;
        NameLocalizationKey = systemName;
        DescriptionLocalizationKey = systemName;
    }

    public string SystemName { get; }
    public string NameLocalizationKey { get; }
    public string DescriptionLocalizationKey { get; }
}