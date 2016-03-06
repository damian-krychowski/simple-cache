# simple-cache
The goal of the SimpleCache is to simplify data caching. It provides special data structures which can aggregate entities by given expression - indexes. Each created index is updated during the cache update - this approach allows to increase reading speed for the price of writing speed and used memory. Index can be used to access stored entities directly or it can be combined with other indexes to perfom more complex queries.

Nuget: https://www.nuget.org/packages/simple-cache/

#Entity
Entity represents an object identified by an unique id (Guid). The object should implement `IEntity` interface to be stored in the cache.

```c#
public interface IEntity
{
    Guid Id { get; }
}

class Dog : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}

class Cat : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Dog WorstEnemy { get; set; }
}
```

#Cache
###Factory
Dedicated factory should be used to prepare the cache.

```c#
var cache = CacheFactory.CreateFor<Cat>()
.WithIndex(cat => cat.Name)
.WithSortedIndex(cat => cat.WorstEnemy.Age, cat=> cat.Name).Ascending()
.BuildUp();
```
The empty cache for cats was created. There is also a method to create the cache with some initial data:

```c#
.BuildUp(new[]{ cat1, cat2, cat3});
```
###Indexes
There were two indexes registered. First one `cat => cat.Name` groups the cats by their name. It allows us to get quick access to the group of cats with a specific name, like:

```c#
var garfields = cache.Index(cat=> cat.Name).Get("Garfield");
```
The second index groups the cats by the age of their worst dog enemy. Moreover, cats stored in the index will be sorted ascendingly by their name. So for example, let's cache the following collection:

```c#
var garfield = new Cat {Name = "Garfield", WorstEnemy = new Dog { Age = 1}};
var tom = new Cat {Name = "Tom", WorstEnemy = new Dog { Age = 2}},
var grumpy = new Cat {Name = "Grumpy", WorstEnemy = new Dog { Age = 1}}

cache.AddOrUpdateRange(new[] { garfield, tom, grumpy } );
```
Now, after getting the cats with enemies of age 1, we will acquire following results

```c#
var catsWithEnemy = cache.Index(cat => cat.WorstEnemy.Age).Get(1);

catsWithEnemy.Should()
    .BeEquivalentTo(garfield, grumpy)
    .And
    .BeInAscendingOrder(cat => cat.Name); //Garfield, Grumpy
```
###Undefined indexing value
What would happen if there would be cats without a dog enemy? Let's add some to the cache:

```c#
var tiger = new Cat {Name = "Tiger", WorstEnemy = null};
var bella = new Cat {Name = "Bella", WorstEnemy = null};

cache.AddOrUpdate(tiger);
cache.AddOrUpdate(bella);
```

Each index has storage dedicated for cases like this. To acquire all entities which couldn't have the index expression evaluated for any reason, there is prepared special method:

```c#
var catsWithoutEnemy = cache.Index(cat => cat.WorstEnemy.Age).GetWithUndefined();

catsWithoutEnemy.Should()
    .BeEquivalentTo(bella, tiger)
    .And
    .BeInAscendingOrder(cat => cat.Name); //Bella, Tiger
```

For sorted indexes the entities with undefined indexing value are sorted as well.

###Quering
SimpleCache provides methods to perform more complicated queries using many indexes at once. Let's consider following set of cached data:

```c#
var garfield = new Cat {Name = "Garfield", WorstEnemy = new Dog { Age = 1}};
var tom = new Cat {Name = "Tom", WorstEnemy = new Dog { Age = 2}},
var grumpy = new Cat {Name = "Grumpy", WorstEnemy = new Dog { Age = 3}}
var tiger = new Cat {Name = "Tiger", WorstEnemy = null};
var bella = new Cat {Name = "Bella", WorstEnemy = null};
```
The task is to find all cats with name longer than four letters and with enemy older than 1 year. There are two ways to acomplish it with prepared cache. The first one is to use the query.

```c#
var cache = CacheFactory.CreateFor<Cat>()
.WithIndex(cat => cat.Name)
.WithIndex(cat => cat.WorstEnemy.Age)
.BuildUp(new[] { garfield, tom, grumpy, tiger, bella });

var cats = cache.Query()
    .Where(cat => cat.Name, name => name.Length > 4)
    .Where(cat => cat.WorstEnemy.Age, age => age > 1)
    .ToList();
```

Indexes and conditions, which they have to fulfill, should be specified explicitly to build the query. For example:

```c#
.Where(cat => cat.Name, name => name.Length > 4)
```
It means, that `cat => cat.Name` index's keys should be longer than 4 letters. The query allows to combine more general indexes to acquire specific data. The intersection of each indexed entity represents the final collection. This, however, takes some time to be computed. But there is another way.

```c#
var cache = CacheFactory.CreateFor<Cat>()
.WithIndex(cat => cat.Name.Lengh > 4 && cat.WorstEnemy.Age > 1)
.BuildUp(new[] { garfield, tom, grumpy, tiger, bella });

var cats = cache
    .Index(cat => cat.Name.Lengh > 4 && cat.WorstEnemy.Age > 1)
    .Get(true);
```
The second example presents solution faster than the first one. There is only one index registered, so the cache will use less memory as well.
