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
            CollectionAssert.AreEquivalent(Enumerable.Empty<Dog>(), sut.Items);
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
            CollectionAssert.AreEquivalent(new[] { dog }, sut.Items );
        }

        [Test]
        public void Can_add_range_of_entities()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            //Act
            sut.AddOrUpdateRange(new[] { dog1, dog2, dog3 });

            //Assert
           CollectionAssert.AreEquivalent(new[] { dog1, dog2, dog3 }, sut.Items);
         }

        [Test]
        public void Can_build_up_with_some_entities()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1, dog2, dog3 }, sut.Items);
        }

        [Test]
        public void Can_clear()
        {
            //Arrange
            var dog1 = new Dog {Id = Guid.NewGuid(), Name = "Tony"};
            var dog2 = new Dog {Id = Guid.NewGuid(), Name = "Andrew"};
            var dog3 = new Dog {Id = Guid.NewGuid(), Name = "John"};
            
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new [] {dog1, dog2, dog3});

            //Act
            sut.Clear();

            //Assert
            CollectionAssert.AreEquivalent(Enumerable.Empty<Dog>(), sut.Items);
        }

        [Test]
        public void Can_remove_entity()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.TryRemove(dog1);

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog2, dog3 }, sut.Items );
        }

        [Test]
        public void Can_remove_entity_by_id()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.TryRemove(dog1.Id);

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog2, dog3 }, sut.Items);
        }

        [Test]
        public void Can_remove_entities_range()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.TryRemoveRange(new[] { dog1, dog2 });

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog3 }, sut.Items);
        }

        [Test]
        public void Can_remove_entities_range_by_ids()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.TryRemoveRange(new[] { dog1.Id, dog2.Id });

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog3 }, sut.Items);
        }

        [Test]
        public void Can_update_entity()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            var updatedDog1 = new Dog {Id = dog1.Id, Name = "New Tony"};

            //Act
            sut.AddOrUpdate(updatedDog1);

            //Assert
            CollectionAssert.AreEquivalent(new[] { updatedDog1, dog2, dog3 }, sut.Items);
        }

        [Test]
        public void Can_update_entities_range()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            var updatedDog1 = new Dog { Id = dog1.Id, Name = "New Tony" };
            var updatedDog2 = new Dog { Id = dog2.Id, Name = "New Andrew" };

            //Act
            sut.AddOrUpdateRange(new[] {updatedDog1, updatedDog2});

            //Assert
            CollectionAssert.AreEquivalent(new[] { updatedDog1, updatedDog2, dog3 }, sut.Items);
        }
    }
}
