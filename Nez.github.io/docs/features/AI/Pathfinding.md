---
id: Pathfinding
title: Pathfinding
---

Nez provides three pathfinding algorithms out of the box: Breadth First Search, Dijkstra (weighted) and Astar (weighted with heuristic). All three are well suited for not only grid based graphs but also graphs of any type. Which algorithm should you use? That depends on your specific graph and needs. The next sections will go into a bit more detail on each of the algorithms so that you can intelligently decide which to use.

It should be noted that none of the search algorithms require the nodes searched to be locations. Pathfinding doesn't care about what data it is finding a path for. All it needs to know is the node and that node's neighbors (edges). The nodes can be anything at all: paths in a dialog tree, actions for AI (Astar is used by the Goal Oriented Action Planner) or even strings (see below for an example). Edges can be traversable one way or both ways.

The Graph interfaces for Breadth First Search, Dijkstra and Astar are all fully generic so you get to decide what data your nodes need. In the simplest case the nodes can be a `Point` (see `UnweightedGridGraph`, `WeightedGridGraph` and `AstarGridGraph` for examples). If your nodes need a bunch of precomputed data you can use any class that you want for them. This allows you to precompute (offline or at map load time) any data that you might need for the actual path search.



## Breadth First Search
Often called flood fill when used on a grid, Breadth First Search uses an expanding frontier that radiates out from the start position visiting all neighbor nodes on the way. When it reaches the goal it stops the search and returns the path. If no valid path is found null is returned. Breadth First Search is well suited for graphs that have uniform traversal cost between edges.

To implement Breadth First Search all you have to do is satisfy the single-method interface `IUnweightedGraph<T>`. The `UnweightedGraph<T>` is a concrete implementation that you can use as well (an example is below). The `UnweightedGridGraph` is also a concrete implementation that can be used directly or as a starting point for grid based graphs. It works out of the box for TiledMaps as well.

Lets take a look at a functional example of a node based graph of plain old strings. This illustrates how pathfinding can be used to solve non-spatial based problems.


```csharp
// create an UnweightedGraph with strings as nodes
var graph = new UnweightedGraph<string>();
	
// add a set of 5 nodes and edges for each
graph.AddEdgesForNode( "a", new string[] { "b" } ); // a→b
graph.AddEdgesForNode( "b", new string[] { "a", "c", "d" } ); // b→a, b→c, b→d
graph.AddEdgesForNode( "c", new string[] { "a" } ); // c→a
graph.AddEdgesForNode( "d", new string[] { "e", "a" } ); // d→e, d→a
graph.AddEdgesForNode( "e", new string[] { "b" } ); // e→b

// calculate a path from "c" to "e". The result is c→a→b→d→e, which we can confirm by looking at the edge comments above.
var path = BreadthFirstPathfinder.Search( graph, "c", "e" );
```


## Dijkstra (aka Weighted)
Dijkstra expands on Breadth First Search by introducing a cost for each edge that the algorithm takes into account when pathfinding. This lends you more control over the pathfinding process. You can add edges with higher costs (moving along a road vs moving through a mud bog for example) to provide a hint that the algorithm should choose the lowest cost path. The algorithm will ask you for a cost to get from one node to another and you can choose to return any value that makes sense. It will then use the information to find not a only a path to the goal but the lowest cost path.

To implement Dijkstra you have to provide a graph that implements the interface `IWeightedGraph<T>`. The `WeightedGridGraph` is a concrete implementation that can be used directly or as a starting point for grid based graphs. It works out of the box for TiledMaps.

Below is an example of using the `WeightedGridGraph` with a TiledMap layer. A graph will be generated that uses empty space in the TiledMap as traversable nodes and any space with a tile is considered a wall. Some weighted nodes are added to take advantage of Dijkstra's specialty in dealing with nodes of non-uniform weight.


```csharp

var graph = new WeightedGridGraph( tiledMapCollisionLayer );

// add some weighted nodes
graph.WeightedNodes.Add( new Point( 3, 3 ) );
graph.WeightedNodes.Add( new Point( 3, 4 ) );
graph.WeightedNodes.Add( new Point( 4, 3 ) );
graph.WeightedNodes.Add( new Point( 4, 4 ) );

// calculate the path
var path = graph.Search( new Point( 3, 4 ), new Point( 15, 17 ) );
```


## Astar
Astar is probably the most well known of all pathfinding algorithms. It differs from Dijkstra in that it introduces a heuristic for each edge that the algorithm takes into account when pathfinding. This lets you add some interesting elements such as edges with higher costs (moving along a road vs moving through a mud bog for example.) The algorithm will ask you for a cost to get from one node to another and you can choose to return any value that makes sense. It will then use the information to find not a only a path to the goal but the lowest cost path.

To implement Astar you have to provide a graph that implements the interface `IAstarGraph<T>`. The `AstarGridGraph` is a concrete implementation that can be used directly or as a starting point for grid based graphs. It works out of the box for TiledMaps.

Below is an example of using the `AstarGridGraph` with a TiledMap layer. A graph will be generated that uses empty space in the TiledMap as traversable nodes and any space with a tile is considered a wall. Some weighted nodes are added to take advantage of Astar's specialty in dealing with nodes of non-uniform weight.


```csharp

var graph = new AstarGridGraph( tiledMapCollisionLayer );

// add some weighted nodes
graph.WeightedNodes.Add( new Point( 3, 3 ) );
graph.WeightedNodes.Add( new Point( 3, 4 ) );
graph.WeightedNodes.Add( new Point( 4, 3 ) );
graph.WeightedNodes.Add( new Point( 4, 4 ) );

// calculate the path
var path = graph.Search( new Point( 3, 4 ), new Point( 15, 17 ) );
```
