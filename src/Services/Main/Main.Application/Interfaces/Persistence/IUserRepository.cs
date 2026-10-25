﻿﻿using Application.Common.Interfaces.Repositories;
using Main.Entities.User;

namespace Main.Application.Interfaces.Persistence;

public interface IUserRepository : IRepository<User, Guid>
{

	Task<User?> GetUserByPrimaryEmailAsync(
		string email,
		Criteria<User>? criteria = null,
		CancellationToken cancellationToken = default);

	Task<User?> GetUserByLoginAsync(
		string login,
		bool isEmail,
		Criteria<User>? criteria = null,
		CancellationToken cancellationToken = default);
}
