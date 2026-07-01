using Exceptions.Base.Localized;

namespace Application.Common.Exceptions;

public class SettingNotFound(string systemName)
    : LocalizedNotFoundException(
        "setting.not.found",
        new { SystemName = systemName },
        [systemName]);