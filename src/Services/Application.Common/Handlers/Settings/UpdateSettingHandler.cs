using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.NamedObject;

namespace Application.Common.Handlers.Settings;

public record UpdateSettingCommand(string SettingName, string Json) : ICommand<UpdateSettingResult>;

public record UpdateSettingResult;

public class UpdateSettingHandler(
    INamedObjectRegistry<SettingDefinitionNamedObjectBase> registry
) : ICommandHandler<UpdateSettingCommand, UpdateSettingResult>
{
    public async Task<UpdateSettingResult> Handle(
        UpdateSettingCommand request,
        CancellationToken cancellationToken)
    {
        var settingDefinition = registry.GetBySystemName(request.SettingName);
        await settingDefinition.UpdateSettingAsync(request.Json, cancellationToken);
        return new UpdateSettingResult();
    }
}