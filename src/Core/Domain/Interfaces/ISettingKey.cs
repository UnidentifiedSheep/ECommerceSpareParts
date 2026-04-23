namespace Domain.Interfaces;

public interface ISettingKey<TSelf> where TSelf : ISettingKey<TSelf>
{
    static abstract string SettingName { get; }
}