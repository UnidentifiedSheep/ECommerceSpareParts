using Core.Entities;

namespace Core.Interfaces.DbRepositories;

public interface IUserEmailRepository
{
    Task<bool> EmailTaken(string email, CancellationToken ct = default);
}