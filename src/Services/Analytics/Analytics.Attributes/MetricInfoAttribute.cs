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
    /// This constructor sets all fields as system name. Adds .description postfix for DescriptionKey
    /// and .name for NameKey
    /// </summary>
    /// <param name="systemName"></param>
    public MetricInfoAttribute(string systemName)
    {
        SystemName = systemName.Trim();
        NameLocalizationKey = SystemName + ".name";
        DescriptionLocalizationKey = SystemName + ".description";
    }

    public string SystemName { get; }
    public string NameLocalizationKey { get; }
    public string DescriptionLocalizationKey { get; }
}