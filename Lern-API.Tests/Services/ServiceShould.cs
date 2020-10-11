using System;
using System.Threading.Tasks;
using Lern_API.Repositories;
using Lern_API.Services;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class ServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_All_Entities(ILogger<Service<Entity, IRepository<Entity>>> logger)
        {
            var repository = new Mock<IRepository<Entity>>();
            repository.Setup(x => x.All());

            var service = new Service<Entity, IRepository<Entity>>(logger, repository.Object);
            await service.GetAll();

            repository.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Entity(ILogger<Service<Entity, IRepository<Entity>>> logger, Guid id)
        {
            var repository = new Mock<IRepository<Entity>>();
            repository.Setup(x => x.Get(id));

            var service = new Service<Entity, IRepository<Entity>>(logger, repository.Object);
            await service.Get(id);

            repository.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Create_Entity(ILogger<Service<Entity, IRepository<Entity>>> logger, Entity entity)
        {
            var repository = new Mock<IRepository<Entity>>();
            repository.Setup(x => x.Create(entity));

            var service = new Service<Entity, IRepository<Entity>>(logger, repository.Object);
            await service.Create(entity);

            repository.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Entity(ILogger<Service<Entity, IRepository<Entity>>> logger, Entity entity)
        {
            var repository = new Mock<IRepository<Entity>>();
            repository.Setup(x => x.Update(entity, null));

            var service = new Service<Entity, IRepository<Entity>>(logger, repository.Object);
            await service.Update(entity, null);

            repository.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public async Task Delete_Entity(ILogger<Service<Entity, IRepository<Entity>>> logger, Entity entity)
        {
            var repository = new Mock<IRepository<Entity>>();
            repository.Setup(x => x.Delete(entity));

            var service = new Service<Entity, IRepository<Entity>>(logger, repository.Object);
            await service.Delete(entity);

            repository.VerifyAll();
        }
    }
}
