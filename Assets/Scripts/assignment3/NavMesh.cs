using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity.VisualScripting;
using Unity.XR.Oculus.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class NavMesh : MonoBehaviour
{
    // implement NavMesh generation here:
    //    the outline are Walls in counterclockwise order
    //    iterate over them, and if you find a reflex angle
    //    you have to split the polygon into two
    //    then perform the same operation on both parts
    //    until no more reflex angles are present
    //
    //    when you have a number of polygons, you will have
    //    to convert them into a graph: each polygon is a node
    //    you can find neighbors by finding shared edges between
    //    different polygons (or you can keep track of this while 
    //    you are splitting)

    List<List<Wall>> polygons = new List<List<Wall>>();

    public Graph MakeNavMesh(List<Wall> outline)
    {
        polygons.Clear();

        Debug.Log(polygons.Count);
        Split(outline);
        Debug.Log(polygons.Count);

        Graph g = new Graph();
        g.all_nodes = new List<GraphNode>();

        int id = 0;
        foreach (List<Wall> polygon in polygons)
        {
            g.all_nodes.Add(new GraphNode(id, polygon));
            id += 1;
        }

        foreach (GraphNode node in g.all_nodes)
        {
            int edge = 0;
            foreach (Wall wall in node.GetPolygon())
            {
                foreach (GraphNode node2 in g.all_nodes)
                {
                    if (node2.GetID() != node.GetID())
                    {
                        foreach (Wall wall2 in node2.GetPolygon())
                        {
                            if (wall.Same(wall2))
                            {
                                node.AddNeighbor(node2, edge);
                            }
                        }
                    }
                }
                edge += 1;
            } 
        }

        return g;
    }

    public void Split(List<Wall> outline)
    {
        
        for (int i = 0; i < outline.Count; i++)
        {
            Wall first = outline[i];
            Wall second = outline[(i + 1)%outline.Count];
            Wall newWall;
            if (Vector3.Dot(first.normal, second.direction) < 0)
            {
                for (int j = i + 2; j < outline.Count; j++) // go through ALL POINTS
                {
                    bool crosses = false;
                    bool createsReflex = false;
                    for (int k = 0; k < outline.Count; k++)
                    {
                        Wall checkWall = outline[k];
                        if (checkWall.Crosses(first.end, outline[j].end)) 
                        {
                            Debug.Log("crosses");
                            crosses = true;
                            break;
                        }
                    }

                    if (Vector3.Dot(first.normal, (outline[j].end - first.end).normalized) < 0 || Vector3.Dot(outline[j].normal, (first.end - outline[j].end).normalized) < 0) /////////
                    {
                        Debug.Log("creates reflex");
                        createsReflex = true;
                    }

                    if (!crosses && !createsReflex)
                    {
                        List<Wall> polygon1;
                        List<Wall> polygon2;

                        newWall = new Wall(first.end, outline[j].end);

                        if (j > i)
                        {
                            polygon1 = outline.GetRange(i, j - i);
                            polygon1.Add(newWall);

                            polygon2 = outline.GetRange(j, outline.Count - j);
                            if (i == 0)
                            {
                                polygon2.Add(outline[0]);
                            }
                            else
                            {
                                polygon2.AddRange(outline.GetRange(0, i - 1));
                            }
                            polygon2.Add(newWall);
                        }
                        else
                        {
                            polygon1 = outline.GetRange(j, i - j);
                            polygon1.Add(newWall);

                            polygon2 = outline.GetRange(i, outline.Count - i);
                            if (j == 0)
                            {
                                polygon2.Add(outline[0]);
                            }
                            else
                            {
                                polygon2.AddRange(outline.GetRange(0, j - 1));
                            }
                            polygon2.Add(newWall);
                        }

                        Split(polygon1);
                        Split(polygon2);

                        Debug.Log("returning");
                        return;
                    }
                }
                if (i >= 2)
                {
                    for (int j = 0; j < i - 1; j ++)
                    {
                        bool crosses = false;
                        bool createsReflex = false;
                        for (int k = 0; k < outline.Count; k++)
                        {
                            Wall checkWall = outline[k];
                            if (checkWall.Crosses(first.end, outline[j].end)) 
                            {
                                Debug.Log("crosses");
                                crosses = true;
                                break;
                            }
                        }

                        if (Vector3.Dot(first.normal, (outline[j].end - first.end).normalized) < 0 || Vector3.Dot(outline[j].normal, (first.end - outline[j].end).normalized) < 0) /////////
                        {
                            Debug.Log("creates reflex");
                            createsReflex = true;
                        }

                        if (!crosses && !createsReflex)
                        {
                            Debug.Log("starting polygon creation");
                            List<Wall> polygon1;
                            List<Wall> polygon2;

                            newWall = new Wall(first.end, outline[j].end);

                            if (j > i)
                            {
                                polygon1 = outline.GetRange(i, j - i);
                                polygon1.Add(newWall);

                                polygon2 = outline.GetRange(j, outline.Count - j);
                                polygon2.AddRange(outline.GetRange(0, i - 1));
                                polygon2.Add(newWall);
                            }
                            else
                            {
                                polygon1 = outline.GetRange(j, i - j);
                                polygon1.Add(newWall);

                                polygon2 = outline.GetRange(i, outline.Count - i);
                                polygon2.AddRange(outline.GetRange(0, j - 1));
                                polygon2.Add(newWall);
                            }

                            Split(polygon1);
                            Split(polygon2);

                            Debug.Log("returning");
                            return;
                        }
                    }
                }
            }
        }

        polygons.Add(outline);
    }

    List<Wall> outline;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EventBus.OnSetMap += SetMap;
    }

    // Update is called once per frame
    void Update()
    {
       

    }

    public void SetMap(List<Wall> outline)
    {
        Graph navmesh = MakeNavMesh(outline);
        if (navmesh != null)
        {
            Debug.Log("got navmesh: " + navmesh.all_nodes.Count);
            EventBus.SetGraph(navmesh);
        }
    }

    


    
}
