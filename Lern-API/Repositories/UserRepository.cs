using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lern_API.Models;
using Lern_API.Utilities;
using NullGuard;
using PetaPoco;

namespace Lern_API.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
    }

    public class UserRepository : Repository, IUserRepository
    {
        public UserRepository(ILogger logger, IDatabase database) : base(logger, database)
        {
        }

        public IEnumerable<User> All()
        {
            return RunOrDefault(() => Database.Fetch<User>());
        }

        public async Task<IEnumerable<User>> AllAsync()
        {
            return await RunOrDefault(async () => await Database.FetchAsync<User>());
        }

        [return: AllowNull]
        public User Get(int id)
        {
            return RunOrDefault(() => Database.SingleOrDefault<User>(id));
        }

        [return: AllowNull]
        public async Task<User> GetAsync(int id)
        {
            return await RunOrDefault(async () => await Database.SingleOrDefaultAsync<User>(id));
        }

        public int Create(User user)
        {
            return Convert.ToInt32(RunOrDefault(() => Database.Insert(user)));
        }

        public async Task<int> CreateAsync(User user)
        {
            return Convert.ToInt32(await RunOrDefault(async () => await Database.InsertAsync(user)));
        }

        public bool Update(User user)
        {
            return RunOrDefault(() => Database.Update(user)) == user.Id;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            return await RunOrDefault(async () => await Database.UpdateAsync(user)) == user.Id;
        }

        public bool Delete(User user)
        {
            return RunOrDefault(() => Database.Delete<User>(user)) == user.Id;
        }

        public async Task<bool> DeleteAsync(User user)
        {
            return await RunOrDefault(() => Database.DeleteAsync<User>(user)) == user.Id;
        }
    }
}
