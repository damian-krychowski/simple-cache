using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using SimpleCache.Builder;

namespace SimpleCache.ConcurrencyTests
{
    [TestFixture]
    internal class CacheConcurrentAccessTests
    {
        class Dog : IEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Test]
        public void Can_one_entity_be_taken_concurrently_from_cache()
        {
            //Arrange
            var results = new ConcurrentDictionary<int, Dog>();

            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new [] {dog1, dog2, dog3});        

            //Act
            new ThreadsRunner<Dog>()

                .Run(() => sut.GetEntity(dog1.Id))
                    .Threads(1000)
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

            var dog1 = new Dog {Id = Guid.NewGuid(), Name = "Tony", Age = 1};
            var dog2 = new Dog {Id = Guid.NewGuid(), Name = "Andrew", Age = 2};
            var dog3 = new Dog {Id = Guid.NewGuid(), Name = "John", Age = 3};

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Age >= 2)
                .BuildUp(new[] {dog1, dog2, dog3});

            //Act
             new ThreadsRunner<List<Dog>>()

                .Run(() => sut.Index(dog => dog.Age >= 2).Get(true))
                    .Threads(1000)
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

            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony", Age = 1 };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew", Age = 2 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John", Age = 3 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Age >= 2)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
             new ThreadsRunner<List<Dog>>()

                .Run(() => sut.Index(dog => dog.Age >= 2).Get(true))
                    .Threads(500)
                    .StoreResult(trueResults)

                .Run(()=> sut.Index(dog => dog.Age >= 2).Get(false))
                    .Threads(500)
                    .StoreResult(falseResults)

                .StartAndWaitAll();

            //Assert
            foreach (var value in trueResults.Values)
            {
                value.ShouldAllBeEquivalentTo(new[] { dog2, dog3 });
            }

            foreach (var value in falseResults.Values)
            {
                value.ShouldAllBeEquivalentTo(new[] { dog1 });
            }
        }

        [Test]
        public void Can_items_from_index_be_read_or_removed_simultaneously()
        {
            //Arrange
            var trueResults = new ConcurrentDictionary<int, List<Dog>>();
            var falseResults = new ConcurrentDictionary<int, List<Dog>>();

            var dogs = new List<Dog>();

            for (int i = 0; i < 1000; i++)
            {
                var dog1 = new Dog {Id = Guid.NewGuid(), Name = "Tony", Age = 1};
                var dog2 = new Dog {Id = Guid.NewGuid(), Name = "Andrew", Age = 2};
                var dog3 = new Dog {Id = Guid.NewGuid(), Name = "John", Age = 3};

                dogs.AddRange(new[] {dog1, dog2, dog3});
            }

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Age >= 2)
                .BuildUp(dogs);

            //Act
            var threadsRunner = new ThreadsRunner<List<Dog>>()
                .Run(() => sut.Index(dog => dog.Age >= 2).Get(true))
                    .Threads(5000)
                    .StoreResult(trueResults);

            threadsRunner.StartWithoutWaiting();

            foreach (var dog in dogs)
            {
                sut.Remove(dog.Id);
            }

            threadsRunner.WaitAll();

            sut.Items.Should().BeEmpty();
        }
    }
}
