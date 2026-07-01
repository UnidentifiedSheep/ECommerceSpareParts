namespace Internal.Integration.Core.Interfaces.Common;

public interface ICommonClient
{
    IJobNode JobNode { get; }
    ISettingNode SettingNode { get; }
}