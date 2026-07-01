using Application.Common.Dtos;
using Application.Common.Interfaces.Cqrs;
using Application.Common.Interfaces.NamedObject;
using Application.Common.NamedObject;
using Localization.Abstractions.Interfaces;

namespace Application.Common.Handlers.Settings;

public record GetSettingsQuery : IQuery<GetSettingsResult>;

public record GetSettingsResult(IReadOnlyList<SettingDto> Settings);

public class GetSettingsHandler(
    INamedObjectRegistry<SettingDefinitionNamedObjectBase> registry,
    IScopedLocalizedJsonSerializer jsonSerializer,
    IScopedStringLocalizer localizer
) : IQueryHandler<GetSettingsQuery, GetSettingsResult>
{
    public async Task<GetSettingsResult> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var result = new List<SettingDto>();

        foreach (var definition in registry.All)
            result.Add(
                new SettingDto
                {
                    SystemName = definition.SystemName,
                    Name = definition.GetLocalizedName(localizer),
                    Description = definition.GetLocalizedDescription(localizer),
                    InputData = jsonSerializer.SerializeMetadata(definition.InputSettingType),
                    OutputMetadata = jsonSerializer.SerializeMetadata(definition.OutputSettingType),
                    OutputData = await definition.GetOutputJsonAsync(cancellationToken)
                });

        return new GetSettingsResult(result);
    }
}