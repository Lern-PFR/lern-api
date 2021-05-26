using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Lern_API.DataTransferObjects.Requests;
using Lern_API.Models;
using Lern_API.Services;
using Lern_API.Tests.Attributes;
using Lern_API.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lern_API.Tests.Services
{
    public class DatabaseServiceShould
    {
        [Theory]
        [AutoMoqData]
        public async Task Get_All_Entities(List<User> entities)
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            await context.Users.AddRangeAsync(entities);
            await context.SaveChangesAsync();

            var result = await service.GetAll();

            result.Should().NotBeNull().And.BeEquivalentTo(entities);
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Single_Entity_Or_Null(List<User> entities, User target)
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            await context.Users.AddRangeAsync(entities);
            await context.Users.AddAsync(target);
            await context.SaveChangesAsync();

            var result = await service.Get(target.Id);
            var invalidResult = await service.Get(Guid.Empty);

            result.Should().NotBeNull().And.BeEquivalentTo(target);
            invalidResult.Should().BeNull();
        }

        [Theory]
        [AutoMoqData]
        public async Task Get_Entity_With_Its_Children(Subject entity, List<Module> children)
        {
            entity.Modules.Clear();

            children.ForEach(x =>
            {
                x.Concepts.Clear();
                x.SubjectId = entity.Id;
            });

            var context = TestSetup.SetupContext();
            var service = new DatabaseService<Subject, SubjectRequest>(context);

            await context.Modules.AddRangeAsync(children);
            await context.Subjects.AddAsync(entity);
            await context.SaveChangesAsync();

            var result = await service.Get(entity.Id);

            result.Should().NotBeNull().And.BeEquivalentTo(entity);
            result.Modules.Should().BeEquivalentTo(children);
        }

        [Theory]
        [AutoMoqData]
        public async Task Create_Entity(UserRequest request)
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            var result = await service.Create(request);

            result.Should().NotBeNull().And.BeEquivalentTo(request);
            context.Users.FirstOrDefault().Should().BeEquivalentTo(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Update_Entity(User entity, UserRequest request)
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            await context.Users.AddAsync(entity);
            await context.SaveChangesAsync();

            var result = await service.Update(entity.Id, request);

            result.Should().NotBeNull().And.BeEquivalentTo(request);
            context.Users.FirstOrDefault().Should().BeEquivalentTo(result);
        }

        [Theory]
        [AutoMoqData]
        public async Task Delete_Entity(List<User> entities, User target)
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            await context.Users.AddRangeAsync(entities);
            await context.Users.AddAsync(target);
            await context.SaveChangesAsync();

            var result = await service.Delete(target.Id);

            result.Should().BeTrue();
            context.Users.Any(x => x.Id == target.Id).Should().BeFalse();
        }

        [Theory]
        [AutoMoqData]
        public async Task Return_False_On_Invalid_Delete(List<User> entities)
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            await context.Users.AddRangeAsync(entities);
            await context.SaveChangesAsync();

            var result = await service.Delete(Guid.Empty);

            result.Should().BeFalse();
        }

        [Theory]
        [AutoMoqData]
        public async Task Check_If_Entity_Exists(List<User> entities, User target)
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            await context.Users.AddRangeAsync(entities);
            await context.Users.AddAsync(target);
            await context.SaveChangesAsync();

            var result = await service.Exists(target.Id);
            var invalidResult = await service.Exists(Guid.Empty);

            result.Should().BeTrue();
            invalidResult.Should().BeFalse();
        }

        [Theory]
        [AutoMoqData]
        public async Task Execute_Custom_Async_Transactions(User entity)
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            var result = await service.ExecuteTransaction(async set => await set.AddAsync(entity));

            result.Should().NotBeNull();
            result.Entity.Should().BeEquivalentTo(entity);
        }

        [Theory]
        [AutoMoqData]
        public async Task Execute_Custom_Transactions(User entity)
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            var result = await service.ExecuteTransaction(set => set.Add(entity));

            result.Should().NotBeNull();
            result.Entity.Should().BeEquivalentTo(entity);
        }

        [Fact]
        public async Task Return_Null_On_Failed_Transaction()
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            var result = await service.ExecuteTransaction(set =>
            {
                
                throw new InvalidOperationException();
#pragma warning disable CS0162 // Code inaccessible détecté
                return set.ToList();
#pragma warning restore CS0162 // Code inaccessible détecté
            });

            result.Should().BeNull();
        }

        [Fact]
        public async Task Return_Null_On_Failed_Async_Transaction()
        {
            var context = TestSetup.SetupContext();
            var service = new DatabaseService<User, UserRequest>(context);

            var result = await service.ExecuteTransaction(async set =>
            {
                
                throw new InvalidOperationException();
#pragma warning disable CS0162 // Code inaccessible détecté
                return await set.ToListAsync();
#pragma warning restore CS0162 // Code inaccessible détecté
            });

            result.Should().BeNull();
        }
    }
}
