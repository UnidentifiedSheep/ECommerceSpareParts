namespace Domain.Interfaces;

public interface ISetting<TSelf> where TSelf : ISetting<TSelf>
{
	static abstract string SettingName { get; }

	static abstract TSelf Default { get; }
}
