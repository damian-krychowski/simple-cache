using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using SimpleCache.Builder;
using SimpleCache.ConcurrencyTests.ThreadsRunner;

namespace SimpleCache.ConcurrencyTests
{
    [TestFixture]
    internal class CacheConcurrentAccessTests
    {
        class Dog : IEntity
        {
            public Guid Id { get; }
            public string Name { get; set; }
            public int Age { get; set; }

            public Dog()
            {
                Id = Guid.NewGuid();
            }
        }

        [Test]
        public void Can_one_entity_be_taken_concurrently_from_cache()
        {
            //Arrange
            var results = new ConcurrentDictionary<int, Dog>();

            var dog1 = new Dog {Name = "Tony"};
            var dog2 = new Dog {Name = "Andrew"};
            var dog3 = new Dog {Name = "John"};

            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp(new[] {dog1, dog2, dog3});

            //Act
            ThreadsRunnerFactory.Create()
                .PlanExecution(() => sut.GetEntity(dog1.Id))
                    .Threads(10)
                    .StoreResult(results)
                .StartAndWaitAll();

            //Assert
            foreach (var value in results.Values)
            {
                value.Should().Be(dog1);
            }
        }

        [Test]
        public void Can_index_be_used_to_concurrently_get_items_with_one_key()
        {
            //Arrange
            var results = new ConcurrentDictionary<int, List<Dog>>();

            var dog1 = new Dog {Name = "Tony", Age = 1};
            var dog2 = new Dog {Name = "Andrew", Age = 2};
            var dog3 = new Dog {Name = "John", Age = 3};

            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Age >= 2)
                .BuildUp(new[] {dog1, dog2, dog3});

            //Act
            ThreadsRunnerFactory.Create()
                .PlanExecution(() => sut.Index(dog => dog.Age >= 2).Get(true))
                    .Threads(10)
                    .StoreResult(results)
                .StartAndWaitAll();

            //Assert
            foreach (var value in results.Values)
            {
                value.ShouldAllBeEquivalentTo(new[] {dog2, dog3});
            }
        }

        [Test]
        public void Can_index_be_used_to_concurrently_get_items_with_different_keys()
        {
            //Arrange
            var trueResults = new ConcurrentDictionary<int, List<Dog>>();
            var falseResults = new ConcurrentDictionary<int, List<Dog>>();

            var dog1 = new Dog {Name = "Tony", Age = 1};
            var dog2 = new Dog {Name = "Andrew", Age = 2};
            var dog3 = new Dog {Name = "John", Age = 3};

            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Age >= 2)
                .BuildUp(new[] {dog1, dog2, dog3});

            //Act
            ThreadsRunnerFactory.Create()
                .PlanExecution(() => sut.Index(dog => dog.Age >= 2).Get(true))
                    .Threads(10)
                    .StoreResult(trueResults)
                .PlanExecution(() => sut.Index(dog => dog.Age >= 2).Get(false))
                    .Threads(10)
                    .StoreResult(falseResults)
                .StartAndWaitAll();

            //Assert
            foreach (var value in trueResults.Values)
            {
                value.ShouldAllBeEquivalentTo(new[] {dog2, dog3});
            }

            foreach (var value in falseResults.Values)
            {
                value.ShouldAllBeEquivalentTo(new[] {dog1});
            }
        }

        [Test]
        public void Can_perform_many_concurrent_cached_items_removing()
        {
            //Arrange
            var dogs = new List<Dog>();

            for (int i = 0; i < 30; i++)
            {
                var dog1 = new Dog { Name = "Tony", Age = 1 };
                var dog2 = new Dog { Name = "Andrew", Age = 2 };
                var dog3 = new Dog { Name = "John", Age = 3 };

                dogs.AddRange(new[] { dog1, dog2, dog3 });
            }

            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Age >= 2)
                .BuildUp(dogs);

            //Act
            ThreadsRunnerFactory.Create()
                .PlanExecution((index) => sut.Remove(dogs[index].Id))
                    .Threads(90)
                .StartAndWaitAll();

            //Assert
            sut.Items.Should().BeEmpty();
        }

        [Test]
        public void Can_items_from_index_be_read_or_removed_simultaneously()
        {
            //Arrange
            var trueResults = new ConcurrentDictionary<int, List<Dog>>();
            var dogs = new List<Dog>();

            for (int i = 0; i < 30; i++)
            {
                var dog1 = new Dog { Name = "Tony", Age = 1 };
                var dog2 = new Dog { Name = "Andrew", Age = 2 };
                var dog3 = new Dog { Name = "John", Age = 3 };

                dogs.AddRange(new[] { dog1, dog2, dog3 });
            }

            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Age >= 2)
                .BuildUp(dogs);

            //Act
            ThreadsRunnerFactory.Create()
                .PlanExecution((index) => sut.Remove(dogs[index].Id))
                    .Threads(90)
                .PlanExecution(() => sut.Index(dog => dog.Age >= 2).Get(true))
                    .Threads(10)
                    .StoreResult(trueResults)

                .StartAndWaitAll();

            //Assert
            sut.Items.Should().BeEmpty();
        }
    }
}
