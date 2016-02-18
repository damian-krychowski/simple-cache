using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using NUnit.Framework;
using SimpleCache.Builder;
using SimpleCache.Exceptions;

namespace SimpleCache.Tests
{
    [TestFixture]
    internal class QueryTests
    {
        public enum Sex
        {
            Male, Female
        }

        public class Teacher: IEntity
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public Sex Sex { get; set; }
            public string ResponsibleForClasses { get; set; }
            public Guid Id { get; set; }


            public Teacher(string responsibleForClasses, string name, int age, Sex sex)
            {
                Name = name;
                Age = age;
                Sex = sex;
                ResponsibleForClasses = responsibleForClasses;
                Id= Guid.NewGuid();
            }
        }

        [Test]
        public void Can_query_with_one_index_used()
        {
            //Arrange
            var teacher1 = new Teacher("Mathematics", "Arthur", 25, Sex.Male);
            var teacher2 = new Teacher("Mathematics", "Tom", 35, Sex.Male);
            var teacher3 = new Teacher("English", "Andrew", 35, Sex.Male);
            var teacher4 = new Teacher("English", "Bob", 45, Sex.Male);
            var teacher5 = new Teacher("Biology", "John", 55, Sex.Male);
            var teacher6 = new Teacher("Mathematics", "Jessica", 25, Sex.Female);
            var teacher7 = new Teacher("Mathematics", "Andrea", 35, Sex.Female);
            var teacher8 = new Teacher("English", "Kate", 35, Sex.Female);
            var teacher9 = new Teacher("English", "Sandra", 45, Sex.Female);
            var teacher10 = new Teacher("Biology", "Lara", 55, Sex.Female);
            
            var sut = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .WithIndex(teacher => teacher.ResponsibleForClasses)
                .WithIndex(teacher => teacher.Age)
                .WithIndex(teacher => teacher.Name)
                .WithIndex(teacher => teacher.Sex)
                .BuildUp(new[] {teacher1, teacher2, teacher3, teacher4, teacher5, teacher6, teacher7, teacher8, teacher9, teacher10});

            //Act
            var result = sut.Query()
                .Where(t => t.ResponsibleForClasses, classes => classes == "Mathematics")
                .ToList();

            //Assert
            CollectionAssert.AreEquivalent(new[] {teacher1, teacher2, teacher6, teacher7}, result);
        }

        [Test]
        public void Can_query_with_many_indexes_used()
        {
            //Arrange
            var teacher1 = new Teacher("Mathematics", "Arthur", 25, Sex.Male);
            var teacher2 = new Teacher("Mathematics", "Tom", 35, Sex.Male);
            var teacher3 = new Teacher("English", "Andrew", 35, Sex.Male);
            var teacher4 = new Teacher("English", "Bob", 45, Sex.Male);
            var teacher5 = new Teacher("Biology", "John", 55, Sex.Male);
            var teacher6 = new Teacher("Mathematics", "Jessica", 25, Sex.Female);
            var teacher7 = new Teacher("Mathematics", "Andrea", 35, Sex.Female);
            var teacher8 = new Teacher("English", "Kate", 35, Sex.Female);
            var teacher9 = new Teacher("English", "Sandra", 45, Sex.Female);
            var teacher10 = new Teacher("Biology", "Lara", 55, Sex.Female);

            var sut = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .WithIndex(teacher => teacher.ResponsibleForClasses)
                .WithIndex(teacher => teacher.Age)
                .WithIndex(teacher => teacher.Name)
                .WithIndex(teacher => teacher.Sex)
                .BuildUp(new[] { teacher1, teacher2, teacher3, teacher4, teacher5, teacher6, teacher7, teacher8, teacher9, teacher10 });

            //Act
            var result = sut.Query()
                .Where(t => t.ResponsibleForClasses, classes => classes == "Mathematics")
                .Where(t=>t.Age, age=> age>30)
                .Where(t=>t.Name, name=> name.EndsWith("a"))
                .ToList();

            //Assert
            CollectionAssert.AreEquivalent(new[] { teacher7 }, result);
        }

        [Test]
        public void Can_query_with_one_temporary_index_used()
        {
            //Arrange
            var teacher1 = new Teacher("Mathematics", "Arthur", 25, Sex.Male);
            var teacher2 = new Teacher("Mathematics", "Tom", 35, Sex.Male);
            var teacher3 = new Teacher("English", "Andrew", 35, Sex.Male);
            var teacher4 = new Teacher("English", "Bob", 45, Sex.Male);
            var teacher5 = new Teacher("Biology", "John", 55, Sex.Male);
            var teacher6 = new Teacher("Mathematics", "Jessica", 25, Sex.Female);
            var teacher7 = new Teacher("Mathematics", "Andrea", 35, Sex.Female);
            var teacher8 = new Teacher("English", "Kate", 35, Sex.Female);
            var teacher9 = new Teacher("English", "Sandra", 45, Sex.Female);
            var teacher10 = new Teacher("Biology", "Lara", 55, Sex.Female);

            var sut = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .BuildUp(new[] { teacher1, teacher2, teacher3, teacher4, teacher5, teacher6, teacher7, teacher8, teacher9, teacher10 });

            //Act
            var result = sut.Query()
                .UseTemporaryIndexes()
                .Where(t => t.ResponsibleForClasses, classes => classes == "Mathematics")
                .ToList();

            //Assert
            CollectionAssert.AreEquivalent(new[] { teacher1, teacher2, teacher6, teacher7 }, result);
        }

        [Test]
        public void Can_query_with_many_temporary_indexes_used()
        {
            //Arrange
            var teacher1 = new Teacher("Mathematics", "Arthur", 25, Sex.Male);
            var teacher2 = new Teacher("Mathematics", "Tom", 35, Sex.Male);
            var teacher3 = new Teacher("English", "Andrew", 35, Sex.Male);
            var teacher4 = new Teacher("English", "Bob", 45, Sex.Male);
            var teacher5 = new Teacher("Biology", "John", 55, Sex.Male);
            var teacher6 = new Teacher("Mathematics", "Jessica", 25, Sex.Female);
            var teacher7 = new Teacher("Mathematics", "Andrea", 35, Sex.Female);
            var teacher8 = new Teacher("English", "Kate", 35, Sex.Female);
            var teacher9 = new Teacher("English", "Sandra", 45, Sex.Female);
            var teacher10 = new Teacher("Biology", "Lara", 55, Sex.Female);

            var sut = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .BuildUp(new[] { teacher1, teacher2, teacher3, teacher4, teacher5, teacher6, teacher7, teacher8, teacher9, teacher10 });

            //Act
            var result = sut.Query()
                .UseTemporaryIndexes()
                .Where(t => t.ResponsibleForClasses, classes => classes == "Mathematics")
                .Where(t => t.Age, age => age > 30)
                .Where(t => t.Name, name => name.EndsWith("a"))
                .ToList();

            //Assert
            CollectionAssert.AreEquivalent(new[] { teacher7 }, result);
        }

        [Test]
        public void Can_query_with_many_indexes_for_items_with_undefined_keys()
        {
            //Arrange
            var teacher1 = new Teacher(null, "Arthur", 25, Sex.Male);
            var teacher2 = new Teacher("Mathematics", "Tom", 35, Sex.Male);
            var teacher3 = new Teacher("English", "Andrew", 35, Sex.Male);
            var teacher4 = new Teacher("English", "Bob", 45, Sex.Male);
            var teacher5 = new Teacher(null, "John", 55, Sex.Male);
            var teacher6 = new Teacher("Mathematics", "Jessica", 25, Sex.Female);
            var teacher7 = new Teacher("Mathematics", "Andrea", 35, Sex.Female);
            var teacher8 = new Teacher(null, "Kate", 35, Sex.Female);
            var teacher9 = new Teacher("English", "Sandra", 45, Sex.Female);
            var teacher10 = new Teacher("Biology", "Lara", 55, Sex.Female);

            var sut = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .WithIndex(teacher => teacher.ResponsibleForClasses)
                .WithIndex(teacher => teacher.Age)
                .WithIndex(teacher => teacher.Name)
                .WithIndex(teacher => teacher.Sex)
                .BuildUp(new[]
                {teacher1, teacher2, teacher3, teacher4, teacher5, teacher6, teacher7, teacher8, teacher9, teacher10});

            //Act
            var result = sut.Query()
                .WhereUndefined(t => t.ResponsibleForClasses)
                .Where(t => t.Sex, sex=> sex == Sex.Male)
                .ToList();

            //Assert
            CollectionAssert.AreEquivalent(new[] { teacher1, teacher5 }, result);
        }

        [Test]
        public void Should_throw_when_unkown_index_was_used()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .BuildUp();

            //Act & Assert
            Assert.Throws<IndexNotFoundException>(() => sut.Query()
                .Where(t => t.ResponsibleForClasses, classes => classes == "Mathematics")
                .ToList());
        }

        [Test]
        public void Should_not_throw_on_unkown_index_if_temporary_indexes_are_turned_on()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .BuildUp();

            //Act & Assert
            Assert.DoesNotThrow(() => sut.Query()
                .UseTemporaryIndexes()
                .Where(t => t.ResponsibleForClasses, classes => classes == "Mathematics")
                .ToList());
        }

        [Test]
        public void Should_throw_when_index_selector_is_null()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .BuildUp();

            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.Query()
                .Where<string>(null, classes => classes == "Mathematics")
                .ToList());
        }

        [Test]
        public void Should_throw_when_value_condition_is_null()
        {
            //Arrange
            var sut = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .BuildUp();

            //Act & Assert
            Assert.Throws<ArgumentNullException>(() => sut.Query()
                .Where(t => t.ResponsibleForClasses, null)
                .ToList());
        }

        [Explicit]
        [Test]
        public void Should_query_faster_for_defined_indexes_than_for_temporary()
        {
            //Arrange
            var aLotOfTeachers = new List<Teacher>();

            for (int i = 0; i < 50000; i++)
            {
                aLotOfTeachers.Add(new Teacher(null, "Arthur", 25, Sex.Male));
                aLotOfTeachers.Add(new Teacher("Mathematics", "Tom", 35, Sex.Male));
                aLotOfTeachers.Add(new Teacher("English", "Andrew", 35, Sex.Male));
                aLotOfTeachers.Add(new Teacher("English", "Bob", 45, Sex.Male));
                aLotOfTeachers.Add(new Teacher(null, "John", 55, Sex.Male));
                aLotOfTeachers.Add(new Teacher("Mathematics", "Jessica", 25, Sex.Female));
                aLotOfTeachers.Add(new Teacher("Mathematics", "Andrea", 35, Sex.Female));
                aLotOfTeachers.Add(new Teacher(null, "Kate", 35, Sex.Female));
                aLotOfTeachers.Add(new Teacher("English", "Sandra", 45, Sex.Female));
                aLotOfTeachers.Add(new Teacher("Biology", "Lara", 55, Sex.Female));
            }
            
            var sutWithIndexes = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .WithIndex(teacher => teacher.ResponsibleForClasses)
                .WithIndex(teacher => teacher.Age)
                .WithIndex(teacher => teacher.Name)
                .WithIndex(teacher => teacher.Sex)
                .BuildUp(aLotOfTeachers);

            var sutWithoutIndexes = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .BuildUp(aLotOfTeachers);
            
            //Act

            var indexedStartTime = DateTime.Now;

            var indexedResult = sutWithIndexes.Query()
                .Where(t => t.ResponsibleForClasses, classes => classes == "Mathematics")
                .Where(t => t.Age, age => age > 30)
                .Where(t => t.Name, name => name.EndsWith("a"))
                .ToList();

            var indexedLenght = DateTime.Now - indexedStartTime;

            var notIndexedStartTime = DateTime.Now;

            var notIndexedResult = sutWithoutIndexes.Query()
                .UseTemporaryIndexes()
                .Where(t => t.ResponsibleForClasses, classes => classes == "Mathematics")
                .Where(t => t.Age, age => age > 30)
                .Where(t => t.Name, name => name.EndsWith("a"))
                .ToList();

            var notIndexedLenght = DateTime.Now - notIndexedStartTime;

            //Assert
            Assert.That(indexedLenght, Is.LessThan(notIndexedLenght));
        }

        [Explicit]
        [Test]
        public void Should_query_faster_for_defined_indexes_than_for_linq_query()
        {
            //Arrange
            var aLotOfTeachers = new List<Teacher>();

            for (int i = 0; i < 50000; i++)
            {
                aLotOfTeachers.Add(new Teacher(null, "Arthur", 25, Sex.Male));
                aLotOfTeachers.Add(new Teacher("Mathematics", "Tom", 35, Sex.Male));
                aLotOfTeachers.Add(new Teacher("English", "Andrew", 35, Sex.Male));
                aLotOfTeachers.Add(new Teacher("English", "Bob", 45, Sex.Male));
                aLotOfTeachers.Add(new Teacher(null, "John", 55, Sex.Male));
                aLotOfTeachers.Add(new Teacher("Mathematics", "Jessica", 25, Sex.Female));
                aLotOfTeachers.Add(new Teacher("Mathematics", "Andrea", 35, Sex.Female));
                aLotOfTeachers.Add(new Teacher(null, "Kate", 35, Sex.Female));
                aLotOfTeachers.Add(new Teacher("English", "Sandra", 45, Sex.Female));
                aLotOfTeachers.Add(new Teacher("Biology", "Lara", 55, Sex.Female));
            }

            var sut = CacheBuilderFactory.CreateCacheBuilder<Teacher>()
                .WithIndex(t => t.ResponsibleForClasses == "Mathematics" && t.Age > 30 && t.Name.EndsWith("a"))
                .BuildUp(aLotOfTeachers);

            //Act

            var indexedStartTime = DateTime.Now;

            var indexedResult = sut.Query()
                .Where(t => t.ResponsibleForClasses == "Mathematics" && t.Age > 30 && t.Name.EndsWith("a"), result=> result)
                .ToList();

            var indexedLenght = DateTime.Now - indexedStartTime;

            var linqStartTime = DateTime.Now;

            var linqResult = aLotOfTeachers
                .Where(t => t.ResponsibleForClasses == "Mathematics" && t.Age > 30 && t.Name.EndsWith("a"))
                .ToList();
           
            var linqLenght = DateTime.Now - linqStartTime;

            //Assert
            Assert.That(indexedLenght, Is.LessThan(linqLenght));
        }
    }
}
