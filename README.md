# simple-cache
The goal of the SimpleCache is to simplify data caching. It provides special data structures which can aggregate entities by given expression - indexes. Each created index is updated during the cache update - this approach allows to increase reading speed for the price of writing speed and used memory. Index can be used to access stored entities directly or it can be combined with other indexes to perfom more complex queries.

#Entity
Entity represents an object identified by an unique id (Guid). To store an object in the cache it has to implement `IEntity` interface.

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
To prepare the cache one should use dedicated factory

```c#
var cache = CacheFactory.CreateFor<Cat>()
.WithIndex(cat => cat.Name)
.WithSortedIndex(cat => cat.WorstEnemy.Age, cat=> cat.Name).Ascending()
.BuildUp();
```
The empty cache for cats was created. To created a cache with some initial data:

```c#
.BuildUp(new[]{ cat1, cat2, cat3});
```
###Indexes
There were two indexes registered. First one `cat => cat.Name` groups the cats by their name. It allows us to get quick `O(1)` access to the group of cats with a specific name, like:

```c#
var garfields = cache.Index(cat=> cat.Name).Get("Garfield");
```
The second index groups the cats by the age of their worst dog enemy. Moreover, cats stored in the index will be sorted ascendingly by their name. So for example, let's cache the following collection:

```c#
var cat1 = new Cat {Name = "Garfield", WorstEnemy = new Dog { Age = 1}};
var cat2 = new Cat {Name = "Tom", WorstEnemy = new Dog { Age = 2}},
var cat3 = new Cat {Name = "Grumpy", WorstEnemy = new Dog { Age = 1}}

cache.AddOrUpdateRange(new[] { cat1, cat2, cat3 } );
```
Now, after getting the cats with enemies of age 1, we will acquire following results

```c#
var catsWithEnemy = cache.Index(cat => cat.WorstEnemy.Age).Get(1);

catsWithEnemy.Should()
    .BeEquivalentTo(cat1, cat3)
    .And
    .BeInAscendingOrder(cat => cat.Name); //Garfield, Grumpy
```
###Undefined indexing value
