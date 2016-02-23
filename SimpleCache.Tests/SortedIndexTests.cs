using System;
using FluentAssertions;
using NUnit.Framework;
using SimpleCache.Builder;
using SimpleCache.Exceptions;

namespace SimpleCache.Tests
{
    [TestFixture]
    class SortedIndexTests
    {
        class Dog : IEntity
        {
            public Guid Id { get; }
            public string Name { get; set; }
            public string Breed { get; set; }
            public int Age { get; set; }

            public Dog()
            {
                Id = Guid.NewGuid();
            }

            public Dog(Guid id)
            {
                Id = id;
            }
        }

        [Test]
        public void Can_register_ascending_sorted_index()
        {
            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithSortedIndex(dog=> dog.Breed, dog=>dog.Age).Ascending()
                .BuildUp();

            //Assert
            sut.ContainsIndexOn(dog => dog.Breed)
                .Should()
                .BeTrue();
        }

        [Test]
        public void Can_register_descending_sorted_index()
        {
            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithSortedIndex(dog => dog.Breed, dog => dog.Age).Descending()
                .BuildUp();

            //Assert
            sut.ContainsIndexOn(dog => dog.Breed)
                .Should()
                .BeTrue();
        }


        [Test]
        public void Can_build_up_with_some_entities_ascending()
        {
            //Arrange
            var dog1 = new Dog { Breed = "Breed A", Name = "Tony", Age = 3};
            var dog2 = new Dog { Breed = "Breed A", Name = "Andrew", Age = 2};
            var dog3 = new Dog { Breed = "Breed B", Name = "John", Age = 1};

            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithSortedIndex(dog => dog.Breed, dog=>dog.Age).Ascending()
                .BuildUp(new[] { dog1, dog2, dog3 });

            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");

            //Assert
            breedADogs
                .Should()
                .BeEquivalentTo(dog1, dog2)
                .And
                .BeInAscendingOrder(dog=>dog.Age);
        }

        [Test]
        public void Can_build_up_with_some_entities_descending()
        {
            //Arrange
            var dog1 = new Dog { Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Breed = "Breed A", Name = "Andrew", Age = 2 };
            var dog3 = new Dog { Breed = "Breed B", Name = "John", Age = 3 };

            //Act
            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithSortedIndex(dog => dog.Breed, dog => dog.Age).Descending()
                .BuildUp(new[] { dog1, dog2, dog3 });

            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");

            //Assert
            breedADogs
                .Should()
                .BeEquivalentTo(dog1,dog2)
                .And
                .BeInDescendingOrder(dog => dog.Age);
        }

        [Test]
        public void Can_index_be_updated()
        {
            //Arrange
            var dog1 = new Dog { Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Breed = "Breed A", Name = "Andrew", Age = 2 };
            var dog3 = new Dog { Breed = "Breed B", Name = "John", Age = 3 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
                .WithSortedIndex(dog => dog.Breed, dog=>dog.Age).Ascending()
                .BuildUp(new[] { dog1, dog2, dog3 });

            var updatedDog1 = new Dog(dog3.Id) { Breed = "Breed C", Name = "Tony", Age = 4};

            //Act
            sut.AddOrUpdate(updatedDog1);
            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");
            var breedCDogs = sut.Index(dog => dog.Breed).Get("Breed C");

            //Assert
            breedADogs
                .Should()
                .BeEquivalentTo(dog1, dog2)
                .And
                .BeInAscendingOrder(dog=>dog.Age);

            breedCDogs
                .Should()
                .BeEquivalentTo(updatedDog1);
        }

        [Test]
        public void Can_item_be_removed_from_index()
        {
            //Arrange
            var dog1 = new Dog { Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Breed = "Breed A", Name = "Andrew", Age = 2 };
            var dog3 = new Dog { Breed = "Breed B", Name = "John", Age = 3 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
               .WithSortedIndex(dog => dog.Breed, dog => dog.Age).Ascending()
               .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.Remove(dog2.Id);
            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");

            //Assert
            breedADogs
                .Should()
                .BeEquivalentTo(dog1);
        }


        [Test]
        public void Can_index_be_rebuilt()
        {
            //Arrange
            var dog1 = new Dog { Breed = "Breed A", Name = "Tony", Age = 1 };
            var dog2 = new Dog { Breed = "Breed A", Name = "Andrew", Age = 2 };
            var dog3 = new Dog { Breed = "Breed A", Name = "John", Age = 3 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
               .WithSortedIndex(dog => dog.Breed, dog => dog.Age).Ascending()
               .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            dog3.Breed = "Changed Breed";
            dog3.Age = 1;

            sut.RebuildIndexes();

            var rebuiltState = sut.Index(dog => dog.Breed).Get("Breed A").ToArray();

            //Assert
            rebuiltState
                .Should()
                .BeEquivalentTo(dog1, dog2)
                .And
                .BeInAscendingOrder(dog => dog.Age);
        }

        [Test]
        public void Can_index_get_with_undefined_indexing_values()
        {
            //Arrange
            var dog1 = new Dog { Breed = null, Name = "Tony", Age = 2 };
            var dog2 = new Dog { Breed = null, Name = "Andrew", Age = 1 };
            var dog3 = new Dog { Breed = "Breed B", Name = "John", Age = 3 };

            var sut = CacheBuilderFactory.CreateCacheBuilder<Dog>()
               .WithSortedIndex(dog => dog.Breed, dog => dog.Age).Ascending()
               .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var nullBreedDogs = sut.Index(dog => dog.Breed).GetWithUndefined();

            //Assert
            nullBreedDogs
                .Should()
                .BeEquivalentTo(dog1, dog2)
                .And
                .BeInAscendingOrder(dog => dog.Age);
        }

    }
}
