using Exceptions.Base.Localized;
using Main.Entities;

namespace Application.Common.Exceptions;

public class JobNotFoundException(Guid id) : LocalizedNotFoundException(
	JobNotFoundMessage.Instance,
	new
	{
		Id = id
	});

public class JobScheduleNotFoundException(Guid id) : LocalizedNotFoundException(
	JobScheduleNotFoundMessage.Instance,
	new
	{
		Id = id
	});
