using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;

public class PathFinder : MonoBehaviour
{
    // Assignment 2: Implement AStar
    //
    // DO NOT CHANGE THIS SIGNATURE (parameter types + return type)
    // AStar will be given the start node, destination node and the target position, and should return 
    // a path as a list of positions the agent has to traverse to reach its destination, as well as the
    // number of nodes that were expanded to find this path
    // The last entry of the path will be the target position, and you can also use it to calculate the heuristic
    // value of nodes you add to your search frontier; the number of expanded nodes tells us if your search was
    // efficient
    //
    // Take a look at StandaloneTests.cs for some test cases
    public static (List<Vector3>, int) AStar(GraphNode start, GraphNode destination, Vector3 target)
    {
        int nodesExpanded = 0;
        // Implement A* here
        List<Vector3> path = new List<Vector3>() { target };

        List<GraphNode> frontier = new List<GraphNode>() { start };

        AStarEntry startEntry = new AStarEntry (start, null, 0, Vector3.Distance(start.GetCenter(), destination.GetCenter()));
        List<AStarEntry> entryList = new List<AStarEntry>() { startEntry };

        while (true)
        {
            if (frontier[0].GetID() == destination.GetID())
            {

            }
            AStarEntry tempEntry = new AStarEntry (null, null, 0, 0);
            foreach (AStarEntry entry in entryList)
            {
                if (frontier[0].GetID() == entry.currentNode.GetID())
                {
                    tempEntry = entry;
                }
            }
            List<GraphNeighbor> neighbors = frontier[0].GetNeighbors();
            frontier.RemoveAt(0); // might not work lol, might need a ref to the specific item
            nodesExpanded += 1;


            foreach (GraphNeighbor neighbor in neighbors)
            {
                int requiresRemoval = -1;
                bool requiresInsertion = true;
                bool requiresEntry = true;

                // create entry for current graph node
                foreach (GraphNode node in frontier)
                {
                    if (neighbor.GetNode().GetID() == node.GetID()) // check if copy
                    {
                        foreach (AStarEntry entry in entryList)
                        {
                            if (node.GetID() == entry.currentNode.GetID())
                            {
                                float neighborFValue = Vector3.Distance(neighbor.GetNode().GetCenter(), tempEntry.currentNode.GetCenter()) + tempEntry.distanceFromStart;
                                if (neighborFValue < entry.distanceFromStart)
                                {
                                    entry.currentNode = neighbor.GetNode();
                                    entry.previousNode = tempEntry;
                                    entry.distanceFromStart = neighborFValue;
                                    requiresRemoval = node.GetID();
                                    requiresEntry = false;
                                }
                                else
                                {
                                    requiresInsertion = false;
                                    requiresEntry = false;
                                }
                                break;
                            }
                        }
                        break;   
                    }
                }

                if (requiresRemoval != 0)
                {
                    GraphNode deleteNode = null;
                    foreach (GraphNode node in frontier)
                    {
                        if (node.GetID() == requiresRemoval)
                        {
                            deleteNode = node;
                            break;
                        }
                    }
                    frontier.Remove(deleteNode);
                }

                if (requiresInsertion)
                {
                    int insertIndex = -1;
                    int i = 0;
                    float neighborFValue = 0;

                    foreach (GraphNode node2 in frontier)
                    {
                        foreach (AStarEntry entry in entryList)
                        {
                            if (node2.GetID() == entry.currentNode.GetID())
                            {
                                neighborFValue = Vector3.Distance(neighbor.GetNode().GetCenter(), tempEntry.currentNode.GetCenter()) + Vector3.Distance(neighbor.GetNode().GetCenter(), destination.GetCenter());
                                if (neighborFValue < entry.distanceFromStart)
                                {

                                }
                                break;
                            }
                        }

                        if (neighborFValue < 0) // Replace 0 with node 2 entry data
                        {
                            insertIndex = i;
                            break;
                        }

                        i += 1;
                    }

                    if (insertIndex == -1)
                    {
                        frontier.Append(neighbor.GetNode());
                    }
                    else
                    {  
                        frontier.Insert(insertIndex, neighbor.GetNode());
                    }
                }
            }
        }

        // return path and number of nodes expanded
        return (path, nodesExpanded);

        

        /* Implement A* here    
        List<Vector3> path = new List<Vector3>() {start.GetCenter()}; //target };

        while (true)
        {
            GraphNeighbor best = start.GetNeighbor(Random.Range(0, start.GetNeighbors().Count));
            path.Add(best.GetNode().GetCenter());
            start = best.GetNode();
            if (start == destination) break;
        }

        path.Add(target);

        // return path and number of nodes expanded
        return (path, 0);

        /*frontiers - {(start, 0, euclideanDistance(target, start))}
         while (true)
            best = frontier.popHighestPrioirity()
            if best == destination {
                for each neighbor of best.naighbors);
                    if not expanded(neighbor) {
                        frontier.add(neighbor. best.d + edge, euc(target, neighbor), best)
                        }
                       }
                    }
                }
            }
        ]
        //Sorted array, create function to add and remove from array
        
        //Find parents (AStarEntry -> node,d,h,priority = h+d, parent)
         */

    }

    public class AStarEntry
    {
        public GraphNode currentNode;
        public AStarEntry previousNode;
        public float distanceFromStart;
        public float distanceFromEnd;

        public AStarEntry(GraphNode currentNode, AStarEntry previousNode, float distanceFromStart, float distanceFromEnd)
        {
            this.currentNode = currentNode;
            this.previousNode = previousNode;
            this.distanceFromStart = distanceFromStart;
            this.distanceFromEnd = distanceFromEnd;
        }
    }

    public Graph graph;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventBus.OnTarget += PathFind;
        EventBus.OnSetGraph += SetGraph;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGraph(Graph g)
    {
        graph = g;
    }

    // entry point
    public void PathFind(Vector3 target)
    {
        if (graph == null) return;

        // find start and destination nodes in graph
        GraphNode start = null;
        GraphNode destination = null;
        foreach (var n in graph.all_nodes)
        {
            if (Util.PointInPolygon(transform.position, n.GetPolygon()))
            {
                start = n;
            }
            if (Util.PointInPolygon(target, n.GetPolygon()))
            {
                destination = n;
            }
        }
        if (destination != null)
        {
            // only find path if destination is inside graph
            EventBus.ShowTarget(target);
            (List<Vector3> path, int expanded) = PathFinder.AStar(start, destination, target);

            Debug.Log("found path of length " + path.Count + " expanded " + expanded + " nodes, out of: " + graph.all_nodes.Count);
            EventBus.SetPath(path);
        }
        

    }

    

 
}
