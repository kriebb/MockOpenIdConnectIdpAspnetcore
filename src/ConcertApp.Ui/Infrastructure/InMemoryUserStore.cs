using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;

namespace ConcertApp.Ui;

public class InMemoryUserStore : IUserStore<IdentityUser>
{
    private readonly ConcurrentDictionary<string, IdentityUser> _users = new();

    public void Dispose()
    {
        _users.Clear();
    }

    public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);

        return Task.FromResult(user.Id);
    }

    public Task<string?> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);

        _users.TryGetValue(user.Id, out var storedUser);
        return Task.FromResult(storedUser?.UserName);
    }

    public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
    {
        user.UserName = userName;
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);

        _users.TryGetValue(user.Id, out var storedUser);
        return Task.FromResult(storedUser?.NormalizedUserName);
    }

    public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
    {
        user.NormalizedUserName = normalizedName;
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);
        return Task.CompletedTask;
    }

    public Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.TryAdd(user.Id, user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.AddOrUpdate(user.Id, user, (key, oldValue) => user);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
    {
        _users.TryRemove(user.Id, out _);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityUser?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<IdentityUser?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        var user = _users.Values.FirstOrDefault(u => u.NormalizedUserName == normalizedUserName);
        return Task.FromResult(user);
    }
}