using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleCache.Builder;

namespace SimpleCache.Tests
{
    [TestFixture]
    class CacheTests
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
        public void Can_get_entity()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var result = sut.GetEntity(dog1.Id);

            //Assert
            Assert.That(result, Is.EqualTo(dog1));
        }

        [Test]
        public void Should_throw_when_getting_entity_not_cached()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            //Act & Assert
            Assert.Throws<KeyNotFoundException>(() => sut.GetEntity(Guid.NewGuid()));
        }

        [Test]
        public void Can_try_to_get_entity()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            Dog entity;
            var result = sut.TryGetEntity(dog1.Id, out entity);

            //Assert
            Assert.That(result, Is.True);
            Assert.That(entity, Is.EqualTo(dog1));
        }

        [Test]
        public void Can_try_to_get_entity_when_not_cached()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            //Act
            Dog entity;
            var result = sut.TryGetEntity(Guid.NewGuid(), out entity);

            //Assert
            Assert.That(result, Is.False);
            Assert.That(entity, Is.Null);
        }

        [Test]
        public void Can_check_if_cached_entity_is_contained()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var result = sut.ContainsEntity(dog1.Id);

            //Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void Can_check_if_not_cached_entity_is_not_contained()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            //Act
            var result = sut.ContainsEntity(Guid.NewGuid());

            //Assert
            Assert.That(result, Is.False);
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
        public void Can_remove_entity_by_id()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.Remove(dog1.Id);

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog2, dog3 }, sut.Items);
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
