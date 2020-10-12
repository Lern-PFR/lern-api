using System.Threading.Tasks;
using Lern_API.Models;
using Microsoft.Extensions.Logging;
using PetaPoco;

namespace Lern_API.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        public Task<User> GetByLogin(string login);
    }

    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ILogger<UserRepository> logger, IDatabase database) : base(logger, database)
        {
        }

        public async Task<User> GetByLogin(string login)
        {
            return await RunOrDefault(async () => await Database.SingleOrDefaultAsync<User>("WHERE nickname = @0 OR email = @0", login));
        }
    }
}
