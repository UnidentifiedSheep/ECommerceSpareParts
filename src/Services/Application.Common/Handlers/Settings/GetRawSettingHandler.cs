using Application.Common.Exceptions;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.NamedObject;

namespace Application.Common.Handlers.Settings;

public record GetRawSettingQuery(string SystemName) : IQuery<GetRawSettingResult>;
public record GetRawSettingResult(string Value);

public class GetRawSettingHandler(
    INamedObjectRegistry<SettingDefinitionNamedObjectBase> registry) : IQueryHandler<GetRawSettingQuery, GetRawSettingResult>
{
    public async Task<GetRawSettingResult> Handle(GetRawSettingQuery request, CancellationToken cancellationToken)
    {
        var settingDefinition = registry.TryGetBySystemName(request.SystemName)
                                ?? throw new SettingNotFound(request.SystemName);

        var setting = await settingDefinition.GetSettingAsync(cancellationToken);
        return new GetRawSettingResult(setting.Json);
    }
}