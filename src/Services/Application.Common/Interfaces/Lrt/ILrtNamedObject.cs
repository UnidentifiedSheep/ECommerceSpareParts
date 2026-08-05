using Abstractions.Interfaces;
using Application.Common.Interfaces.NamedObject;

namespace Application.Common.Interfaces.Lrt;

public interface ILrtNamedObject :
    ILrt,
    ILrtDescriptor,
    ILocalizableNamedObject
{
    IServiceDefinition ServiceDefinition { get; }
}
