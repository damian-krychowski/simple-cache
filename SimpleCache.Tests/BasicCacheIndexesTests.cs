using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleCache.Builder;
using SimpleCache.Exceptions;

namespace SimpleCache.Tests
{
    [TestFixture]
    class BasicCacheIndexesTests
    {
        class Dog : IEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Breed { get; set; }
            public int Age { get; set; }
        }

        [Test]
        public void Should_throw_when_index_not_found()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            //Act & Assert
            Assert.Throws<IndexNotFoundException>(()=>sut.Index1D(dog=> dog.Breed));
        }

        [Test]
        public void Can_register_index1D()
        {
            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex1D(dog => dog.Breed)
                .BuildUp();

            //Assert
            Assert.That(sut.ContainsIndexOn(dog=>dog.Breed), Is.True);
        }

        [Test]
        public void Can_index_entities_with_index1D()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
              .WithIndex1D(dog => dog.Breed)
              .BuildUp();

            var dog1 = new Dog {Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony"};
            var dog2 = new Dog {Id = Guid.NewGuid(), Breed = "Breed A", Name = "Andrew"};
            var dog3 = new Dog {Id = Guid.NewGuid(), Breed = "Breed B", Name = "John"};

            //Act
            sut.AddOrUpdateRange(new[] { dog1, dog2, dog3 });
            var breedADogs = sut.Index1D(dog => dog.Breed).Get("Breed A");

            //Assert
            CollectionAssert.AreEquivalent(breedADogs, new[] {dog1, dog2});
        }

        [Test]
        public void Can_index1D_be_updated()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
              .WithIndex1D(dog => dog.Breed)
              .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "John" };

            var updatedDog1 = new Dog { Id = dog1.Id, Breed = "Breed C", Name = "Tony" };

            sut.AddOrUpdateRange(new[] { dog1, dog2, dog3 });

            //Act
            sut.AddOrUpdate(updatedDog1);
            var breedADogs = sut.Index1D(dog => dog.Breed).Get("Breed A");
            var breedCDogs = sut.Index1D(dog => dog.Breed).Get("Breed C"); 

            //Assert
            CollectionAssert.AreEquivalent(breedADogs, new[] { dog2 });
            CollectionAssert.AreEquivalent(breedCDogs, new[] {updatedDog1});
        }

        [Test]
        public void Can_item_be_removed_from_index1D()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
              .WithIndex1D(dog => dog.Breed)
              .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "John" };

            sut.AddOrUpdateRange(new[] { dog1, dog2, dog3 });

            //Act
            sut.TryRemove(dog1);
            var breedADogs = sut.Index1D(dog => dog.Breed).Get("Breed A");

            //Assert
            CollectionAssert.AreEquivalent(breedADogs, new[] { dog2 });
        }

        [Test]
        public void Can_register_index2d()
        {
            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog => dog.Name)
                .BuildUp();

            //Assert
            Assert.That(sut.ContainsIndexOn(dog => dog.Breed, dog => dog.Name), Is.True);
        }

        [Test]
        public void Can_index_entities_with_index2D()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
              .WithIndex2D(dog => dog.Breed, dog=>dog.Age)
              .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1};
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 1 };

            //Act
            sut.AddOrUpdateRange(new[] { dog1, dog2, dog3 });
            var result = sut.Index2D(dog => dog.Breed, dog=>dog.Age).Get("Breed A", 1);

            //Assert
            CollectionAssert.AreEquivalent(result, new[] { dog1, dog3 });
        }

        [Test]
        public void Can_index2D_be_updated()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog=> dog.Age)
                .BuildUp();

            var dog1 = new Dog {Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1};
            var dog2 = new Dog {Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1};
            var dog3 = new Dog {Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 1};

            var updatedDog1 = new Dog {Id = dog1.Id, Breed = "Breed C", Name = "Tony", Age = 2};

            sut.AddOrUpdateRange(new[] {dog1, dog2, dog3});

            //Act
            sut.AddOrUpdate(updatedDog1);
            var result = sut.Index2D(dog => dog.Breed, dog => dog.Age).Get("Breed A", 1);

            //Assert
            CollectionAssert.AreEquivalent(result, new[] {dog3});
        }

        [Test]
        public void Can_item_be_removed_from_index2D()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
              .WithIndex2D(dog => dog.Breed, dog=>dog.Age)
              .BuildUp();

            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 1 };

            sut.AddOrUpdateRange(new[] { dog1, dog2, dog3 });

            //Act
            sut.TryRemove(dog1);
            var breedADogs = sut.Index2D(dog => dog.Breed, dog=>dog.Age).Get("Breed A", 1);

            //Assert
            CollectionAssert.AreEquivalent(breedADogs, new[] { dog3 });
        }
    }
}
