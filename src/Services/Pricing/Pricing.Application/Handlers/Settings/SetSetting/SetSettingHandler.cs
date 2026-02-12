using System.Text.Json;
using Abstractions.Interfaces.Services;
using Application.Common.Interfaces;
using Contracts.Settings;
using MassTransit;
using MediatR;
using Pricing.Abstractions.Interfaces.DbRepositories;
using Pricing.Entities;

namespace Pricing.Application.Handlers.Settings.SetSetting;

public record SetSettingCommand(string Key, object Value) : ICommand;

public class SetSettingHandler(IUnitOfWork unitOfWork, ISettingsRepository settingsRepository,
    IPublishEndpoint publishEndpoint) 
    : ICommandHandler<SetSettingCommand>
{
    public async Task<Unit> Handle(SetSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await settingsRepository.GetSetting(request.Key,true, cancellationToken);
        var json = JsonSerializer.Serialize(request.Value);

        if (setting == null)
        {
            setting = new Setting { Key = request.Key };
            await unitOfWork.AddAsync(setting, cancellationToken);
        }
        
        setting.Value = json;
        
        await publishEndpoint.Publish(new SettingChangedEvent
        {
            Key = setting.Key,
            Value = setting.Value,
            ChangedAt = DateTime.UtcNow
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}