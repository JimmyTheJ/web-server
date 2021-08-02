using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VueServer.Models.Context;
using VueServer.Models.User;

namespace VueServer.Models.Identity
{
    public class ServerUserStore : IUserStore<WSUser>, IUserRoleStore<WSUser>, IUserPasswordStore<WSUser>, IQueryableUserStore<WSUser>
    {
        private IWSContext context;

        public ServerUserStore(IWSContext context)
        {
            this.context = context;
        }

        public IQueryable<WSUser> Users => context.Users.AsQueryable();

        public Task<IdentityResult> CreateAsync(WSUser user, CancellationToken cancellationToken)
        {
            if (context.Users.Find(user.Id) != null)
            {
                return Task.FromResult(IdentityResult.Failed());
            }

            context.Users.Add(user);
            context.SaveChanges();
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(WSUser user, CancellationToken cancellationToken)
        {
            var toDelete = context.Users.Find(user.Id);

            if (toDelete != null)
            {
                context.Users.Remove(toDelete);
                context.SaveChanges();
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<WSUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return context.Users.Where(x => x.Id == userId).FirstOrDefaultAsync();
        }

        public Task<WSUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return context.Users.Where(x => x.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync();
        }

        public Task<string> GetNormalizedUserNameAsync(WSUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(WSUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(WSUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(WSUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<bool> HasPasswordAsync(WSUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task SetNormalizedUserNameAsync(WSUser user, string normalizedName, CancellationToken cancellationToken)
        {
            if (normalizedName != user.Id.ToUpper())
            {
                user.NormalizedUserName = normalizedName;
            }

            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(WSUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(WSUser user, string userName, CancellationToken cancellationToken)
        {
            user.DisplayName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(WSUser user, CancellationToken cancellationToken)
        {
            WSUser toUpdate = await FindByIdAsync(user.Id, cancellationToken);
            toUpdate.NormalizedUserName = user.NormalizedUserName;
            toUpdate.DisplayName = user.DisplayName;
            toUpdate.PasswordHash = user.PasswordHash;

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

        public Task AddToRoleAsync(WSUser user, string roleName, CancellationToken cancellationToken)
        {
            // TODO: Possibly check if this should be the Role.DisplayName or the Id
            var userRole = context.UserRoles.Where(o => o.UserId == user.Id && o.Role.DisplayName == roleName).FirstOrDefault();

            if (userRole == null)
            {
                WSRole role = context.Roles.Where(o => o.NormalizedName == roleName).FirstOrDefault();

                if (role == null) throw new ArgumentException($"Not a valid role name ('{roleName}')");

                context.UserRoles.Add(new WSUserInRoles(user.Id, role.Id));
            }

            context.SaveChanges();
            return Task.CompletedTask;
        }

        public Task RemoveFromRoleAsync(WSUser user, string roleName, CancellationToken cancellationToken)
        {
            // TODO: Possibly check if this should be the Role.DisplayName or the Id
            var userRole = context.UserRoles.Where(o => o.UserId == user.Id && o.Role.DisplayName == roleName).FirstOrDefault();

            if (userRole != null)
            {
                context.UserRoles.Remove(userRole);
                context.SaveChanges();
            }

            return Task.CompletedTask;
        }

        public async Task<IList<string>> GetRolesAsync(WSUser user, CancellationToken cancellationToken)
        {
            // TODO: Possibly check if this should be the Role.DisplayName or the Id
            return await context.UserRoles.Where(o => o.UserId == user.Id).Select(o => o.Role.DisplayName).ToListAsync();
        }

        public Task<bool> IsInRoleAsync(WSUser user, string roleName, CancellationToken cancellationToken)
        {
            // TODO: Possibly check if this should be the Role.DisplayName or the Id
            return context.UserRoles.Where(o => o.UserId == user.Id && o.Role.DisplayName == roleName).AnyAsync();
        }

        public Task<IList<WSUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            // TODO: Possibly check if this should be the Role.DisplayName or the Id
            return Task.FromResult((IList<WSUser>)context.UserRoles.Where(o => o.Role.DisplayName == roleName).Select(o => o.User).ToList());
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
