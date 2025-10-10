using Main.Core.Entities;
using Main.Core.Models;

namespace Main.Core.Interfaces.DbRepositories;

public interface IUserPhoneRepository
{
    Task<IEnumerable<UserPhone>> GetUserPhonesAsync(Guid userId, int? limit = null, int? offset = null,
        bool track = true, CancellationToken cancellationToken = default);

    Task<UserPhone?> GetUserPhoneAsync(Guid id, bool track = true,
        CancellationToken cancellationToken = default);

    Task<UserPhone?> GetUserPhoneAsync(string phone, bool track = true,
        CancellationToken cancellationToken = default);

    Task<UserPhone?> GetUserPrimaryPhoneAsync(Guid userId, bool track = true,
        CancellationToken cancellationToken = default);

    Task<bool> IsPhoneTakenAsync(string phone, CancellationToken cancellationToken = default);
    Task<int> GetUserPhoneCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UserHasPrimaryPhoneAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserPhoneSummary?> GetUserPhoneSummaryAsync(Guid userId, CancellationToken cancellationToken = default);
}