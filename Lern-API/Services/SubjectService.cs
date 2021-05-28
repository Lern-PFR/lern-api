using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Helpers.JWT;
using Lern_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Lern_API.Services
{
    public interface ISubjectService : IDatabaseService<Subject, SubjectRequest>
    {
        Task<IEnumerable<Subject>> GetMine(CancellationToken token = default);
    }

    public class SubjectService : DatabaseService<Subject, SubjectRequest>, ISubjectService
    {
        public SubjectService(LernContext context, IHttpContextAccessor httpContextAccessor) : base(context, httpContextAccessor)
        {
        }

        public new async Task<IEnumerable<Subject>> GetAll(CancellationToken token = default)
        {
            return await DbSet.Where(x => x.Modules.Any()).ToListAsync(token);
        }

        public async Task<IEnumerable<Subject>> GetMine(CancellationToken token = default)
        {
            var currentUser = HttpContextAccessor.HttpContext.GetUser();

            return await DbSet.Where(x => x.AuthorId == currentUser.Id).ToListAsync(token);
        }
    }
}
