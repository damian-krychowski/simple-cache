using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SimpleCache.Builder;
using SimpleCache.Exceptions;

namespace SimpleCache.Tests
{
    [TestFixture]
    internal class IndexTests
    {
        private class Dog : IEntity
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

        private class Cat : IEntity
        {
            public Guid Id { get;  }
            public Dog WorstEnemy { get; set; }

            public Cat()
            {
                Id = Guid.NewGuid();
            }
        }

        [Test]
        public void Should_throw_when_index_not_found()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp();

            //Act & Assert
            Action act = () => sut.Index(dog => dog.Breed);
            act.ShouldThrow<IndexNotFoundException>();
        }

        [Test]
        public void Can_register_index()
        {
            //Act
            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp();

            //Assert
            sut.ContainsIndexOn(dog => dog.Breed)
                .Should()
                .BeTrue();
        }


        [Test]
        public void Should_throw_when_index_expression_for_contains_index_on_method_is_null()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
                .BuildUp();

            //Act & Assert
            Action act = () => sut.ContainsIndexOn<string>(null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Can_build_up_with_some_entities()
        {
            //Arrange
            var dog1 = new Dog { Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Breed = "Breed B", Name = "John" };

            //Act
            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });

            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");

            //Assert
            breedADogs.ShouldAllBeEquivalentTo(new[] { dog1, dog2 });
        }

        [Test]
        public void Can_get_entities_with_index()
        {
            //Arrange
            var dog1 = new Dog { Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Breed = "Breed B", Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });
            
            //Act
            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");

            //Assert
            breedADogs.ShouldAllBeEquivalentTo(new[] { dog1, dog2 });
        }

        [Test]
        public void Can_index_be_updated()
        {
            //Arrange
            var dog1 = new Dog { Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Breed = "Breed B", Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });

            var updatedDog1 = new Dog(dog1.Id) {Breed = "Breed C", Name = "Tony"};
            
            //Act
            sut.AddOrUpdate(updatedDog1);
            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");
            var breedCDogs = sut.Index(dog => dog.Breed).Get("Breed C");

            //Assert
            breedADogs.ShouldAllBeEquivalentTo(new[] { dog2 });
            breedCDogs.ShouldAllBeEquivalentTo(new[] { updatedDog1 });
        }

        [Test]
        public void Can_item_be_removed_from_index()
        {
            //Arrange
            var dog1 = new Dog { Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Breed = "Breed B", Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            sut.Remove(dog1.Id);
            var breedADogs = sut.Index(dog => dog.Breed).Get("Breed A");

            //Assert
            breedADogs.ShouldAllBeEquivalentTo(new[] { dog2 });
        }

        [Test]
        public void Should_throw_when_index_get_with_null_value()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp();

            //Act & Assert
            Action act = () => sut.Index<string>(null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_when_index_used_with_null_value()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp();

            //Act & Assert
            Action act = () => sut.Index(dog => dog.Breed).Get(null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Can_index_be_rebuilt()
        {
            //Arrange
            var dog1 = new Dog { Breed = "Breed A", Name = "Tony" };
            var dog2 = new Dog { Breed = "Breed A", Name = "Andrew" };
            var dog3 = new Dog { Breed = "Breed A", Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            dog1.Breed = "Changed Breed";
            sut.RebuildIndexes();

            var rebuiltState = sut.Index(dog => dog.Breed).Get("Breed A").ToArray();

            //Assert
            rebuiltState.ShouldAllBeEquivalentTo(new[] { dog2, dog3 });
        }

        [Test]
        public void Can_index_get_with_undefined_indexing_values()
        {
            //Arrange
            var dog1 = new Dog { Breed = null, Name = "Tony" };
            var dog2 = new Dog { Breed = null, Name = "Andrew" };
            var dog3 = new Dog { Breed = "Breed A", Name = "John" };

            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Breed)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var nullBreedDogs = sut.Index(dog => dog.Breed).GetWithUndefined();

            //Assert
            nullBreedDogs.ShouldAllBeEquivalentTo(new[] { dog1, dog2 });
        }

        [Test]
        public void Can_undefined_indexing_values_be_detected_for_deeper_index()
        {
            //Arrange
            var cat1 = new Cat {WorstEnemy = new Dog {Breed = "Breed A", Name = "Tony"}};
            var cat2 = new Cat {WorstEnemy = new Dog {Breed = null, Name = "Andrew"}};
            var cat3 = new Cat {WorstEnemy = null};

            var sut = CacheBuilderFactory.Create<Cat>()
                .WithIndex(cat => cat.WorstEnemy.Breed)
                .BuildUp(new[] {cat1, cat2, cat3});

            //Act
            var result = sut.Index(cat => cat.WorstEnemy.Breed).GetWithUndefined();

            //Assert
            result.ShouldAllBeEquivalentTo(new[] { cat2, cat3 });
        }

        [Test]
        public void Can_index_with_non_nullable_type_values()
        {
            //Arrange
            var dog1 = new Dog { Age = 1 };
            var dog2 = new Dog { Age = 1 };
            var dog3 = new Dog { Age = 2 };

            var sut = CacheBuilderFactory.Create<Dog>()
                .WithIndex(dog => dog.Age)
                .BuildUp(new[] { dog1, dog2, dog3 });

            //Act
            var result = sut.Index(dog => dog.Age).Get(1);

            //Assert
            result.ShouldAllBeEquivalentTo(new[] { dog1, dog2 });
        }

        [Test]
        public void Can_undefined_non_nullable_indexing_values_be_detected_for_deeper_index()
        {
            //Arrange
            var cat1 = new Cat { WorstEnemy = new Dog { Age = 1 } };
            var cat2 = new Cat { WorstEnemy = new Dog() };
            var cat3 = new Cat { WorstEnemy = null };

            var sut = CacheBuilderFactory.Create<Cat>()
                .WithIndex(cat => cat.WorstEnemy.Age)
                .BuildUp(new[] { cat1, cat2, cat3 });

            //Act
            var result = sut.Index(cat => cat.WorstEnemy.Age).GetWithUndefined();

            //Assert
            result.ShouldAllBeEquivalentTo(new[] { cat3 });
        }
    }
}