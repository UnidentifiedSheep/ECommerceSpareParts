using System.Runtime.Serialization;

namespace Core.Enums;

public enum SpamFilterEnum
{
    /// <summary>
    ///     переместить в паку спам
    /// </summary>
    [EnumMember(Value = "move_to_directory")]
    MoveToDirectory,

    /// <summary>
    ///     переслать письмо на другой адрес
    /// </summary>
    [EnumMember(Value = "forward")] Forward,

    /// <summary>
    ///     удалить письмо
    /// </summary>
    [EnumMember(Value = "delete")] Delete,

    /// <summary>
    ///     пометить письмо
    /// </summary>
    [EnumMember(Value = "tag")] Tag
}