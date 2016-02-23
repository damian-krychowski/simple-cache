using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
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
            var sut = CacheBuilderFactory.Create<Dog>()
               .BuildUp();

            //Assert
            sut.Items.Should().BeEmpty();
        }

        [Test]
        public void Can_add_entity()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
               .BuildUp();

            var dog = new Dog {Id = Guid.NewGuid(), Name = "Tony"};

            //Act
            sut.AddOrUpdate(dog);

            //Assert
            sut.Items.ShouldAllBeEquivalentTo(new[] {dog});
        }

        [Test]
        public void Should_throw_when_adding_null_entity()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
               .BuildUp();
            
            //Act & Assert
            Action act = () => sut.AddOrUpdate(null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Can_add_range_of_entities()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            //Act
            sut.AddOrUpdateRange(new[] { dog1, dog2, dog3 });

            //Assert
            sut.Items.ShouldAllBeEquivalentTo(new[] {dog1,dog2,dog3});
         }


        [Test]
        public void Should_throw_when_adding_null_entities_range()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
               .BuildUp();

            //Act & Assert
            Action act = ()=> sut.AddOrUpdateRange(null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Can_get_entity()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var result = sut.GetEntity(dog1.Id);

            //Assert
            result.ShouldBeEquivalentTo(dog1);
        }

        [Test]
        public void Should_throw_when_getting_entity_not_cached()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp();

            //Act & Assert
            Action act = () => sut.GetEntity(Guid.NewGuid());
            act.ShouldThrow<KeyNotFoundException>();
        }

        [Test]
        public void Can_try_to_get_entity()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            Dog entity;
            var result = sut.TryGetEntity(dog1.Id, out entity);

            //Assert
            result.Should().BeTrue();
            entity.ShouldBeEquivalentTo(dog1);
        }

        [Test]
        public void Can_try_to_get_entity_when_not_cached()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp();

            //Act
            Dog entity;
            var result = sut.TryGetEntity(Guid.NewGuid(), out entity);

            //Assert
            result.Should().BeFalse();
            entity.Should().BeNull();
        }

        [Test]
        public void Can_check_if_cached_entity_is_contained()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var result = sut.ContainsEntity(dog1.Id);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void Can_check_if_not_cached_entity_is_not_contained()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp();

            //Act
            var result = sut.ContainsEntity(Guid.NewGuid());

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Can_build_up_with_some_entities()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            //Act
            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Assert
            sut.Items.ShouldAllBeEquivalentTo(new[] {dog1, dog2, dog3});
        }


        [Test]
        public void Can_remove_entity_by_id()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.Remove(dog1.Id);

            //Assert
            sut.Items.ShouldAllBeEquivalentTo(new[] { dog2, dog3 });
        }

        [Test]
        public void Can_update_entity()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            var updatedDog1 = new Dog {Id = dog1.Id, Name = "New Tony"};

            //Act
            sut.AddOrUpdate(updatedDog1);

            //Assert
            sut.Items.ShouldAllBeEquivalentTo(new[] { updatedDog1, dog2, dog3 });
        }

        [Test]
        public void Can_update_entities_range()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp(new[] { dog1, dog2, dog3 });

            var updatedDog1 = new Dog { Id = dog1.Id, Name = "New Tony" };
            var updatedDog2 = new Dog { Id = dog2.Id, Name = "New Andrew" };

            //Act
            sut.AddOrUpdateRange(new[] {updatedDog1, updatedDog2});

            //Assert
            sut.Items.ShouldAllBeEquivalentTo(new[] { updatedDog1, updatedDog2, dog3 });
        }
    }
}
