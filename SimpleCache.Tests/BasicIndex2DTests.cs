using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleCache.Builder;
using SimpleCache.Exceptions;

namespace SimpleCache.Tests
{
    [TestFixture]
    class BasicIndex2DTests
    {
        class Dog : IEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Breed { get; set; }
            public int? Age { get; set; }
        }

        [Test]
        public void Should_throw_when_index_not_found()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            //Act & Assert
            Assert.Throws<IndexNotFoundException>(() => sut.Index2D(dog => dog.Breed, dog=>dog.Age));
        }

        [Test]
        public void Can_register_index()
        {
            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog => dog.Name)
                .BuildUp();

            //Assert
            Assert.That(sut.ContainsIndexOn(dog => dog.Breed, dog => dog.Name), Is.True);
        }

        [Test]
        public void Can_build_up_with_some_entities()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 1 };

            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
               .WithIndex2D(dog => dog.Breed, dog => dog.Age)
               .BuildUp(new[] { dog1, dog2, dog3 });

            var breedADogs = sut.Index2D(dog => dog.Breed, dog => dog.Age).Get("Breed A", 1);

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1, dog3 }, breedADogs);
        }

        [Test]
        public void Can_get_entities_with_index()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 1 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
              .WithIndex2D(dog => dog.Breed, dog=>dog.Age)
              .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var result = sut.Index2D(dog => dog.Breed, dog=>dog.Age).Get("Breed A", 1);

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1, dog3 },result );
        }

        [Test]
        public void Can_clear()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 1 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog => dog.Age)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.Clear();
            var breedADogs = sut.Index2D(dog => dog.Breed, dog=>dog.Age).Get("Breed A", 1);
            var breedBDogs = sut.Index2D(dog => dog.Breed, dog => dog.Age).Get("Breed B", 1);

            //Assert
            CollectionAssert.AreEquivalent(Enumerable.Empty<Dog>(), breedADogs);
            CollectionAssert.AreEquivalent(Enumerable.Empty<Dog>(), breedBDogs);
        }

        [Test]
        public void Can_index_be_updated()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 1 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog=> dog.Age)
                .BuildUp(new[] { dog1, dog2, dog3 });

            var updatedDog1 = new Dog {Id = dog1.Id, Breed = "Breed C", Name = "Tony", Age = 2};

            //Act
            sut.AddOrUpdate(updatedDog1);
            var result = sut.Index2D(dog => dog.Breed, dog => dog.Age).Get("Breed A", 1);

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog3 },result);
        }

        [Test]
        public void Can_item_be_removed_from_index()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 1 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
              .WithIndex2D(dog => dog.Breed, dog=>dog.Age)
              .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.TryRemove(dog1);
            var breedADogs = sut.Index2D(dog => dog.Breed, dog=>dog.Age).Get("Breed A", 1);

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog3 },breedADogs );
        }

        [Test]
        public void Can_index_be_rebuilt()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 1 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
              .WithIndex2D(dog => dog.Breed, dog => dog.Age)
              .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var initialState = sut.Index2D(dog => dog.Breed, dog=>dog.Age).Get("Breed A", 1).ToArray();

            dog1.Age = 2;
            sut.RebuildIndexes();

            var rebuiltState = sut.Index2D(dog => dog.Breed, dog => dog.Age).Get("Breed A", 1).ToArray();

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1, dog3 }, initialState);
            CollectionAssert.AreEquivalent(new[] { dog3 }, rebuiltState);
        }

        [Test]
        public void Should_throw_when_index_get_with_null_value()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog => dog.Age)
                .BuildUp();

            //Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => sut.Index2D<string, int?>(null, null));
        }

        [TestCase(null, null)]
        [TestCase("key", null)]
        [TestCase(null, 1)]
        public void Should_throw_when_index_used_with_null_value(string firstKey, int? secondKey)
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog=>dog.Age)
                .BuildUp();

            //Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => sut.Index2D(dog => dog.Breed, dog => dog.Age).Get(firstKey, secondKey).ToArray());
        }

        [Test]
        public void Can_index_get_with_undefined_first_indexing_values()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = null, Name = "Tony", Age = null };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = null, Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = null };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog=>dog.Age)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var nullBreedDogs = sut.Index2D(dog => dog.Breed, dog=>dog.Age).GetWithFirstUndefined(1);

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog2 }, nullBreedDogs);
        }

        [Test]
        public void Should_throw_on_null_second_key_when_getting_with_undefined_first_indexing_values()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog => dog.Age)
                .BuildUp();

            //Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => sut.Index2D(dog => dog.Breed, dog => dog.Age).GetWithFirstUndefined(null));
        }

        [Test]
        public void Can_index_get_with_undefined_second_indexing_values()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = null, Name = "Tony", Age = null };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = null, Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = null };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog => dog.Age)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var nullBreedDogs = sut.Index2D(dog => dog.Breed, dog => dog.Age).GetWithSecondUndefined("Breed A");

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog3}, nullBreedDogs);
        }

        [Test]
        public void Should_throw_on_null_first_key_when_getting_with_undefined_second_indexing_values()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog => dog.Age)
                .BuildUp();

            //Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => sut.Index2D(dog => dog.Breed, dog => dog.Age).GetWithSecondUndefined(null));
        }

        [Test]
        public void Can_index_get_with_undefined_both_indexing_values()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = null, Name = "Tony", Age = null };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = null, Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = null };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex2D(dog => dog.Breed, dog => dog.Age)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var nullBreedDogs = sut.Index2D(dog => dog.Breed, dog => dog.Age).GetWithBothUndefined();

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1 }, nullBreedDogs);
        }

        [Test]
        public void Can_get_indexed_values_with_first_key()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 2 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
              .WithIndex2D(dog => dog.Breed, dog => dog.Age)
              .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var breedADogs = sut.Index2D(dog => dog.Breed, dog => dog.Age).GetFromFirst("Breed A");

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1, dog3 }, breedADogs);
        }

        [Test]
        public void Can_get_indexed_values_with_second_key()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John", Age = 2 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
              .WithIndex2D(dog => dog.Breed, dog => dog.Age)
              .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var breedADogs = sut.Index2D(dog => dog.Breed, dog => dog.Age).GetFromSecond(1);

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1, dog2 }, breedADogs);
        }
    }
}
