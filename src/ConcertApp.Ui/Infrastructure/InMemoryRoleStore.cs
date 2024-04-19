using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;

namespace ConcertApp.Ui;

public class InMemoryRoleStore : IRoleStore<IdentityRole>
{
    private readonly ConcurrentDictionary<string, IdentityRole> _roles = new();

    public void Dispose()
    {
        _roles.Clear();
    }

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);

        return Task.FromResult(role.Id);
    }

    public Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);

        _roles.TryGetValue(role.Id, out var storedRole);
        return Task.FromResult(storedRole?.Name);
    }

    public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);

        _roles.TryGetValue(role.Id, out var storedRole);
        return Task.FromResult(storedRole?.NormalizedName);
    }

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);
        return Task.CompletedTask;
    }

    public Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.TryAdd(role.Id, role);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.AddOrUpdate(role.Id, role, (key, oldValue) => role);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        _roles.TryRemove(role.Id, out _);
        return Task.FromResult(IdentityResult.Success);
    }

    public Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        _roles.TryGetValue(roleId, out var role);
        return Task.FromResult(role);
    }

    public Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var role = _roles.Values.FirstOrDefault(r => r.NormalizedName == normalizedRoleName);
        return Task.FromResult(role);
    }
}