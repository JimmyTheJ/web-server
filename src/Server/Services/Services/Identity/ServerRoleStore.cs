using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VueServer.Models.Context;
using VueServer.Models.User;

namespace VueServer.Models.Identity
{
    public class ServerRoleStore : IRoleStore<WSRole>
    {
        private IWSContext context;

        public ServerRoleStore(IWSContext context)
        {
            this.context = context;
        }

        public Task<IdentityResult> CreateAsync(WSRole role, CancellationToken cancellationToken)
        {
            context.Roles.Add(role);
            try
            {
                context.SaveChanges();
                return Task.FromResult(IdentityResult.Success);
            }
            catch (Exception)
            {
                return Task.FromResult(IdentityResult.Failed());
            }
        }

        public Task<IdentityResult> DeleteAsync(WSRole role, CancellationToken cancellationToken)
        {
            var toDelete = context.Roles.Find(role.Id);

            if (toDelete != null)
            {
                context.Roles.Remove(toDelete);
                try
                {
                    context.SaveChanges();
                    return Task.FromResult(IdentityResult.Success);
                }
                catch (Exception)
                {
                    return Task.FromResult(IdentityResult.Failed());
                }
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<WSRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return context.Roles.Where(o => o.RoleId == roleId).FirstOrDefaultAsync();
        }

        public Task<WSRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return context.Roles.Where(o => o.NormalizedName == normalizedRoleName).FirstOrDefaultAsync();
        }

        public Task<string> GetNormalizedRoleNameAsync(WSRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(WSRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.RoleId.ToString());
        }

        public Task<string> GetRoleNameAsync(WSRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(WSRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(WSRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(WSRole role, CancellationToken cancellationToken)
        {
            WSRole toUpdate = await FindByIdAsync(role.RoleId.ToString(), cancellationToken);
            toUpdate.NormalizedName = role.NormalizedName;
            toUpdate.Name = role.Name;

            try
            {
                context.SaveChanges();
                return IdentityResult.Success;
            }
            catch (Exception)
            {
                return IdentityResult.Failed();
            }
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
