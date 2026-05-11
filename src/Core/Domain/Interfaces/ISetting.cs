namespace Domain.Interfaces;

public interface ISetting<TSelf> where TSelf : ISetting<TSelf>
{
    static abstract string SettingName { get; }
    public static abstract TSelf Default { get; }
}