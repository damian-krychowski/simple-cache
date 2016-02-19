using System;
using System.Linq;
using NUnit.Framework;
using SimpleCache.Builder;
using SimpleCache.Exceptions;

namespace SimpleCache.Tests
{
    [TestFixture]
    internal class IndexTests
    {
        class Dog : IEntity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Breed { get; set; }
        }

        [Test]
        public void Should_throw_when_index_not_found()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            //Act & Assert
            Assert.Throws<IndexNotFoundException>(() => sut.Index(dog => dog.Breed));
        }

        [Test]
        public void Can_register_index()
        {
            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp();

            //Assert
            Assert.That(sut.ContainsIndexOn(dog => dog.Breed), Is.True);
        }


        [Test]
        public void Should_throw_when_index_expression_for_contains_index_on_methods_is_null()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .BuildUp();

            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.ContainsIndexOn<string>(null));
        }

        [Test]
        public void Can_build_up_with_some_entities()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "John" };

            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });

            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1, dog2 }, breedADogs);
        }

        [Test]
        public void Can_clear()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
               .WithIndex(dog => dog.Breed)
               .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.Clear();
            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");
            var breedBDogs = sut.Index(dog => dog.Breed).Get("Breed B");

            //Assert
            CollectionAssert.AreEquivalent(Enumerable.Empty<Dog>(), breedADogs);
            CollectionAssert.AreEquivalent(Enumerable.Empty<Dog>(), breedBDogs);
        }

        [Test]
        public void Can_get_entities_with_index()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });
            
            //Act
            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1, dog2 }, breedADogs);
        }

        [Test]
        public void Can_index_be_updated()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });

            var updatedDog1 = new Dog { Id = dog1.Id, Breed = "Breed C", Name = "Tony" };
            
            //Act
            sut.AddOrUpdate(updatedDog1);
            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");
            var breedCDogs = sut.Index(dog => dog.Breed).Get("Breed C");

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog2 }, breedADogs);
            CollectionAssert.AreEquivalent(new[] { updatedDog1 }, breedCDogs);
        }

        [Test]
        public void Can_item_be_removed_from_index()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed B", Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.Remove(dog1.Id);
            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog2 }, breedADogs);
        }

        [Test]
        public void Should_throw_when_index_get_with_null_value()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp();

            //Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => sut.Index<string>(null));
        }

        [Test]
        public void Should_throw_when_index_used_with_null_value()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp();

            //Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => sut.Index(dog => dog.Breed).Get(null).ToArray());
        }

        [Test]
        public void Can_index_be_rebuilt()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var initialState = sut.Index(dog => dog.Breed).Get("Breed A").ToArray();

            dog1.Breed = "Changed Breed";
            sut.RebuildIndexes();

            var rebuiltState = sut.Index(dog => dog.Breed).Get("Breed A").ToArray();

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1, dog2, dog3 }, initialState);
            CollectionAssert.AreEquivalent(new[] { dog2, dog3 }, rebuiltState);
        }

        [Test]
        public void Can_index_get_with_undefined_indexing_values()
        {
            //Arrange
            var dog1 = new Dog { Id = Guid.NewGuid(), Breed = null, Name = "Tony" };
            var dog2 = new Dog { Id = Guid.NewGuid(), Breed = null, Name = "Andrew" };
            var dog3 = new Dog { Id = Guid.NewGuid(), Breed = "Breed A", Name = "John" };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var nullBreedDogs = sut.Index(dog => dog.Breed).GetWithUndefined();

            //Assert
            CollectionAssert.AreEquivalent(new[] { dog1, dog2 }, nullBreedDogs);
        }
   }
}