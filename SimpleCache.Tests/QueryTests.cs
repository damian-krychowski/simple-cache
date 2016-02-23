using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
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
            
            var sut = CacheBuilderFactory.Create<Teacher>()
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
            result.ShouldAllBeEquivalentTo(new[] { teacher1, teacher2, teacher6, teacher7 });
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

            var sut = CacheBuilderFactory.Create<Teacher>()
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
            result.ShouldAllBeEquivalentTo(new[] { teacher1, teacher5 });
        }

        [Test]
        public void Should_throw_when_unkown_index_was_used()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Teacher>()
                .BuildUp();

            //Act & Assert
            Action act = () => sut
                .Query()
                .Where(t => t.ResponsibleForClasses, classes => classes == "Mathematics")
                .ToList();

            act.ShouldThrow<IndexNotFoundException>();
        }

        [Test]
        public void Should_throw_when_index_selector_is_null()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Teacher>()
                .BuildUp();

            //Act & Assert
            Action act = () => sut.Query()
                .Where<string>(null, classes => classes == "Mathematics")
                .ToList();

            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_throw_when_value_condition_is_null()
        {
            //Arrange
            var sut = CacheBuilderFactory.Create<Teacher>()
                .BuildUp();

            //Act & Assert
            Action act = () => sut.Query()
                .Where(t => t.ResponsibleForClasses, null)
                .ToList();

           act.ShouldThrow<ArgumentNullException>();
        }
    }
}
