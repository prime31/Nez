---
id: Collections
title: Collections
---

.Net has already implemented the most common Collections, Nez adds several flexible collections, which can help you optimize your game.


## Deque

The deque is a Queue where you can add and remove items at the front as well as at the end with a time complexity of `O(1)`.

### Usage

#### Add and AddBack
The `Add` and `AddBack` functions allows you to add Items to the back of the `Deque`

```cs
deque.AddBack(1);
deque.Add(2); //Add is added to implement ICollection<T> and calls AddBack
deque.AddBack(3);
```

The queue now looks like this
`back [3,2,1] front`

```cs
int a = deque.RemoveFront(); // 1
int b = deque.RemoveFront(); // 2
int c = deque.RemoveFront(); // 3
```

If we had executed this instead
```cs
int a = deque.RemoveBack(); // 3
int b = deque.RemoveBack(); // 2
int c = deque.RemoveBack(); // 1
```
#### AddFront

The `AddFront` function allows you to add Items to the front of the `Deque`.

```cs
var deque = new Deque<int>();
deque.AddFront(1);
deque.AddFront(2);
deque.AddFront(3);
```

The queue now looks like this
`back [1,2,3] front`

```cs
int a = deque.RemoveFront(); // 3
int b = deque.RemoveFront(); // 2
int c = deque.RemoveFront(); // 1
```

If we had executed this instead

```cs
int a = deque.RemoveBack(); // 1
int b = deque.RemoveBack(); // 2
int c = deque.RemoveBack(); // 3
```

### Implementation

The `Deque` is implemented with a circular array

The following insertions
```cs
var deque = new Deque<char>();
deque.AddBack('a');
deque.AddBack('b');
deque.AddBack('c');
```

Result in the following buffer

| Index | 0 | 1 | 2 | 3 | 4 | 5 | .. | 15 |
|-------|---|---|---|---|---|---|----|----|
| Value | a | b | c |   |   |   |    |    |

If we then run `RemoveFront`
```cs
deque.RemoveFront();
```

The buffer will look like this

| Index | 0 | 1 | 2 | 3 | 4 | 5 | .. | 15 |
|-------|---|---|---|---|---|---|----|----|
| Value |   | b | c |   |   |   |    |    |

If we then run `AddFront` twice 
the front index continues on the other side of the buffer

```cs
deque.AddFront('d');
deque.AddFront('e');
```

The buffer will look like this

| Index | 0 | 1 | 2 | 3 | 4 | 5 | .. | 15 |
|-------|---|---|---|---|---|---|----|----|
| Value | d | b | c |   |   |   |    |  e |

By keeping track of the front and the back, the items do not have to be continually shifted around, which makes the add and remove complexity `O(1)`.

## FastList

The `FastList` is a wrapper around an `Array` that auto-expands it when it reaches capacity.

### Usage

```cs
FastList<int> fastList = new FastList<int>();
```

#### Adding items

You can add items with `Add`

```cs
fastList.Add(1);
```

And multiple items at once with `AddRange`

```cs
fastList.AddRange(Enumerable.Range(1,5));
```

#### Removing items

You can remove items based on value. 
This requires traversing the list to find the item and shifting which results in a complexity of `O(n)`

```cs
fastList.Remove(1);
```

You can also remove items based on index, but this still requires the elements to be shifted which results in a complexity of `O(n)`.
```cs
fastList.RemoveAt(0);
```

The fastest method is `RemoveAtWithSwap`. Here the item is swapped with the item at the end of the list and then deleted, which results in a complexity of `O(1)`.
The disadvantage of this method is that the order is no longer correct.
```cs
fastList.RemoveAtWithSwap(1);
```

#### Iterating
When iterating the List, it is important that you use `FastList.Length` instead of the Buffer length

```cs
for (var i = 0; i < fastList.Length; i++)
{
    var item = fastList.Buffer[i];
}
```

## Pair
A `Pair` is a simple mutable structure for managing a two objects

### Usage
Creating a pair
```cs
var twoNumbersPair = new Pair<object>(1, 2);
```

Unlike `Tuples`, `Pairs` are mutable
```cs
twoNumbersPair.First = 3;
twoNumbersPair.Second = 4;
```

You can use the `Clear` method to set pair items to `null`
```cs
twoNumbersPair.Clear();
```

### Alternatives

**`Tuple`**

As an alternative to `Pair`, you can also use the dotnet `Tuples`
```cs
var twoNumbersTuple = new Tuple<int, int>(1, 2);
```
One limitation of `Tuples` is that they are immutable.

**`ValueTuples`**

You can also use `ValueTuples` like this.

```cs
var twoNumbersValueTuple = new ValueTuple<int, int>(1, 2);
twoNumbersValueTuple.Item1 = 3;
twoNumbersValueTuple.Item1 = 4;
```

Or use the `C# 7.0` declaration

```cs
(int, int) twoNumbersValueTuple;
twoNumbersValueTuple.Item1 = 3;
twoNumbersValueTuple.Item1 = 4;
```

```cs
(int first, int second) twoNumbersValueTuple;
twoNumbersValueTuple.first = 3;
twoNumbersValueTuple.second = 4;
```

`ValueTuples` are value types though, so they are passed by value instead of reference. You can partially get around this by using the `ref` keyword.

```cs
void SomeFunction(ref (int first, int second) twoNumbersValueTuple)
{
    twoNumbersValueTuple.first = 3;
    twoNumbersValueTuple.second = 4;
}
```

```cs
(int first, int second) twoNumbersValueTuple = default;
SomeFunction(ref twoNumbersValueTuple);
```

Overview for each situation

|                   | Mutable      | Immutable |
|-------------------|--------------|-----------|
| **ReferenceType** | `Pair`       | `Tuple`   |
| **ValueType**     | `ValueTuple` |           |


## PriorityQueue

The `PriorityQueue` is a data structure that allows you to prioritize elements very efficiently. This is useful for example for algorithms like `A*` and `Dijkstra's Shortest Path`.

### Usage

For this example, I am using SimplePriorityQueue. 
```cs
var queue = new SimplePriorityQueue<int>();
queue.Enqueue(3,3);
queue.Enqueue(2,2);
queue.Enqueue(1,1);
queue.Enqueue(4,4);
queue.Enqueue(5,5);
```
The first parameter of `Enqueue` is the `value`. The second parameter is the `priority`.
For this example, I'll use the same value.


```cs
int value1 = queue.Dequeue(); // 1
int value2 = queue.Dequeue(); // 2
int value3 = queue.Dequeue(); // 3
int value4 = queue.Dequeue(); // 4
int value5 = queue.Dequeue(); // 5
```

As you can see, the values are coming out sorted from the `PriorityQueue`

For understanding the internal workings of the PriorityQueue, I recommend you to read [this](https://www.geeksforgeeks.org/binary-heap/).

