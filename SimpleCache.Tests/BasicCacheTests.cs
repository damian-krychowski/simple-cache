using System;
using System.Linq;
using NUnit.Framework;
using SimpleCache.Builder;

namespace SimpleCache.Tests
{
    [TestFixture]
    class BasicCacheTests
    {
        class Dog : IEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        [Test]
        public void Should_be_empty_when_nothing_is_added()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
               .BuildUp();

            //Assert
            CollectionAssert.AreEquivalent(sut.Items, Enumerable.Empty<Dog>());
        }

        [Test]
        public void Can_add_entity()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
               .BuildUp();

            var dog = new Dog {Id = Guid.NewGuid(), Name = "Tony"};

            //Act
            sut.AddOrUpdate(dog);

            //Assert
            CollectionAssert.AreEquivalent(sut.Items, new[] {dog});
        }

        [Test]
        public void Can_add_range_of_entities()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            var dogs = new[]
            {
                new Dog {Id = Guid.NewGuid(), Name = "Tony"},
                new Dog {Id = Guid.NewGuid(), Name = "Andrew"},
                new Dog {Id = Guid.NewGuid(), Name = "John"}
            };

            //Act
            sut.AddOrUpdateRange(dogs);

            //Assert
           CollectionAssert.AreEquivalent(sut.Items, dogs);
         }

        [Test]
        public void Can_remove_entity()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            var dog1 = new Dog {Id = Guid.NewGuid(), Name = "Tony"};
            var dog2 = new Dog {Id = Guid.NewGuid(), Name = "Andrew"};
            var dog3 = new Dog {Id = Guid.NewGuid(), Name = "John"};

            var dogs = new[] {dog1, dog2, dog3};

            sut.AddOrUpdateRange(dogs);

            //Act
            sut.TryRemove(dog1);

            //Assert
            CollectionAssert.AreEquivalent(sut.Items, new[] {dog2, dog3 });
        }

        [Test]
        public void Can_remove_entity_by_id()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var dogs = new[] { dog1, dog2, dog3 };

            sut.AddOrUpdateRange(dogs);

            //Act
            sut.TryRemove(dog1.Id);

            //Assert
            CollectionAssert.AreEquivalent(sut.Items, new[] { dog2, dog3 });
        }

        [Test]
        public void Can_remove_entities_range()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var dogs = new[] { dog1, dog2, dog3 };

            sut.AddOrUpdateRange(dogs);

            //Act
            sut.TryRemoveRange(new[] { dog1, dog2 });

            //Assert
            CollectionAssert.AreEquivalent(sut.Items, new[] { dog3 });
        }

        [Test]
        public void Can_remove_entities_range_by_ids()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var dogs = new[] { dog1, dog2, dog3 };

            sut.AddOrUpdateRange(dogs);

            //Act
            sut.TryRemoveRange(new[] { dog1, dog2 }.Select(dog=>dog.Id));

            //Assert
            CollectionAssert.AreEquivalent(sut.Items, new[] { dog3 });
        }

        [Test]
        public void Can_update_entity()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var dogs = new[] { dog1, dog2, dog3 };

            sut.AddOrUpdateRange(dogs);

            var updatedDog1 = new Dog {Id = dog1.Id, Name = "New Tony"};

            //Act
            sut.AddOrUpdate(updatedDog1);

            //Assert
            CollectionAssert.AreEquivalent(sut.Items, new[] { updatedDog1, dog2, dog3 });
        }

        [Test]
        public void Can_update_entities_range()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var dogs = new[] { dog1, dog2, dog3 };

            sut.AddOrUpdateRange(dogs);

            var updatedDog1 = new Dog { Id = dog1.Id, Name = "New Tony" };
            var updatedDog2 = new Dog { Id = dog2.Id, Name = "New Andrew" };

            //Act
            sut.AddOrUpdateRange(new[] {updatedDog1, updatedDog2});

            //Assert
            CollectionAssert.AreEquivalent(sut.Items, new[] { updatedDog1, updatedDog2, dog3 });
        }
    }
}
