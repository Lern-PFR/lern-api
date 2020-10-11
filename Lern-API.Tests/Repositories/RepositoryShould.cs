using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lern_API.Repositories;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using PetaPoco;
using Xunit;

namespace Lern_API.Tests.Repositories
{
    public class RepositoryShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Return_Queries_Result(ILogger<Repository<Entity>> logger, IDatabase database, Entity data)
        {
            var repository = new Repository<Entity>(logger, database);

            var result = await repository.RunOrDefault(async () => data);

            Assert.NotNull(result);
            Assert.Equal(data, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_Null_On_Error(ILogger<Repository<Entity>> logger, IDatabase database, Entity data)
        {
            var repository = new Repository<Entity>(logger, database);

            var resultPostgres = await repository.RunOrDefault(async () =>
            {
#pragma warning disable CS0612 // Le type ou le membre est obsolète
                throw new PostgresException();
#pragma warning restore CS0612 // Le type ou le membre est obsolète
#pragma warning disable CS0162 // Code inaccessible détecté
                return data;
#pragma warning restore CS0162 // Code inaccessible détecté
            });

            var resultAggregate = await repository.RunOrDefault(async () =>
            {
                throw new AggregateException();
#pragma warning disable CS0162 // Code inaccessible détecté
                return data;
#pragma warning restore CS0162 // Code inaccessible détecté
            });

            Assert.Null(resultPostgres);
            Assert.Null(resultAggregate);
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_All_Entities(ILogger<Repository<Entity>> logger, List<Entity> entities)
        {
            var database = new Mock<IDatabase>();
            database.Setup(x => x.FetchAsync<Entity>()).ReturnsAsync(entities);

            var repository = new Repository<Entity>(logger, database.Object);

            var result = await repository.All();

            Assert.NotNull(result);
            Assert.Equal(entities.Count, result.Count());
            Assert.True(entities.All(x => result.Any(y => y == x)));
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Entity(ILogger<Repository<Entity>> logger, Entity entity)
        {
            var database = new Mock<IDatabase>();
            database.Setup(x => x.SingleOrDefaultAsync<Entity>(It.IsAny<Guid>())).ReturnsAsync(entity);

            var repository = new Repository<Entity>(logger, database.Object);

            var result = await repository.Get(Guid.NewGuid());

            Assert.NotNull(result);
            Assert.Equal(entity, result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Create_Entity(ILogger<Repository<Entity>> logger, Entity entity)
        {
            var database = new Mock<IDatabase>();
            database.Setup(x => x.InsertAsync(entity));

            var repository = new Repository<Entity>(logger, database.Object);

            await repository.Create(entity);

            database.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Entity(ILogger<Repository<Entity>> logger, Entity entity)
        {
            var database = new Mock<IDatabase>();
            database.Setup(x => x.UpdateAsync(entity));

            var repository = new Repository<Entity>(logger, database.Object);

            await repository.Update(entity);

            database.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Delete_Entity(ILogger<Repository<Entity>> logger, Entity entity)
        {
            var database = new Mock<IDatabase>();
            database.Setup(x => x.DeleteAsync<Entity>(entity));

            var repository = new Repository<Entity>(logger, database.Object);

            await repository.Delete(entity);

            database.VerifyAll();
        }
    }
}
