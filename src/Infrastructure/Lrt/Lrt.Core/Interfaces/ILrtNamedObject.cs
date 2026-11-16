using NamedObject.Core.Interfaces;

namespace Application.Common.Interfaces.Lrt;

public interface ILrtNamedObject : ILrt, ILrtDescriptor, ILocalizableNamedObject;

public interface ILrtNamedObject<in TInputState> : ILrtNamedObject, ILrt<TInputState>
	where TInputState : IInputState;
