---
id: Pooling
title: Pooling
---

When building Games, you often use objects that are quite expensive to create and clean up (GC). These costs can be high enough to affect the game's performance. Nez offers the ObjectPool and ListPool to solve this.

An Object pool is a container of objects that contains a list of objects that are ready to be used. Once an object is taken out of the pool, it is no longer available in the pool until it is put back in.

# Pool

## Example

As an example to show how you can use a `Pool`, I created a simple tree that retrieves its nodes from a `Pool`.

```cs
class SomeNode : IPoolable
{
    public static int InstanceCount = 0;

    public SomeNode()
    {
        InstanceCount++;
    }

    public int Value;

    public SomeNode Left;
    public SomeNode Right;

    public void Reset()
    {
        Value = default;
        Left = default;
        Right = default;
    }
}
```

As you can see in the code above, `SomeNode` inherits `IPoolable`.
IPoolable is an interface used by the pool to reset the object. It is not mandatory to implement this, but it can be useful.

`InstanceCount` is used to find out how many instances have been created.

```cs
class SomeTree
{
    private SomeNode _root;

    public void Add(int value)
    {
        Add(ref _root, value);
    }

    public void Clear()
    {
        foreach (SomeNode someNode in GetNodes(_root))
        {
            Pool<SomeNode>.Free(someNode);
        }
    }

    private void Add(ref SomeNode node, int value)
    {
        if (node == null)
        {
            node = Pool<SomeNode>.Obtain();
            node.Value = value;
        }
        if (value > node.Value)
        {
            Add(ref node.Right, value);
        }
        if (value < node.Value)
        {
            Add(ref node.Left, value);
        }
    }

    private IEnumerable<SomeNode> GetNodes(SomeNode node)
    {
        if (node == null)
            yield break;
        
        foreach (SomeNode leftNode in GetNodes(node.Left))
        {
            yield return leftNode;
        }
        foreach (SomeNode rightNode in GetNodes(node.Right))
        {
            yield return rightNode;
        }
        yield return node;
    }

    public override string ToString()
    {
        return string.Join(",", GetNodes(_root).Select(x => x.Value));
    }
}
```

The tree uses the `Pool` in the `Clear` and `void Add(ref SomeNode node, int value)` methods.

```cs
SomeTree firstTree = new SomeTree();
firstTree.Add(3);
firstTree.Add(2);
firstTree.Add(4);
firstTree.Add(5);
firstTree.Add(1);
string firstResult = firstTree.ToString(); // "1,2,5,4,3"
System.Console.WriteLine(SomeNode.InstanceCount); // 5
```
After creating the tree, 5 nodes were instantiated.

With the `Clear` method we put them back into the pool.
```cs
// Free Nodes to pool
firstTree.Clear();
```

Then we create a new tree.
```cs
SomeTree secondTree = new SomeTree();
secondTree.Add(8);
secondTree.Add(7);
secondTree.Add(4);
secondTree.Add(2);
secondTree.Add(3);
string secondResult = secondTree.ToString(); // "3,2,4,7,8"
System.Console.WriteLine(SomeNode.InstanceCount); // 5
```
no new nodes were created because they where reused from the pool.

## Extra
`Pool` also has a few features that were not present in the example.

### `Pool<>.ClearCache` 
Removes all pooled items.

### `Pool<>.WarmCache`
Add instances to the pool.

### `Pool<>.TrimCache`
Removes items from the pool until the pool is at the desired trim size.


# ListPool

`ListPool` works the same as `Pool`, except that the list is automatically emptied by the `Free` method.