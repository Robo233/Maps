using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public List<Node> graph;

    public static List<Node> AStarSearch(Node startNode, Node endNode)
    {
        HashSet<Node> openSet = new HashSet<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
        Dictionary<Node, float> gScore = new Dictionary<Node, float>();
        Dictionary<Node, float> fScore = new Dictionary<Node, float>();

        openSet.Add(startNode);
        gScore[startNode] = 0;
        fScore[startNode] = HeuristicCostEstimate(startNode, endNode);

        while(openSet.Count > 0)
        {
            Node currentNode = openSet.OrderBy(n => fScore[n]).First();
            //Debug.Log("Current Node: " + currentNode.position + ", fScore: " + fScore[currentNode] + "with number of edges: " + currentNode.edges.Count);

            if(currentNode == endNode)
            {
                //Debug.Log("End node reached!");
                return ReconstructPath(cameFrom, currentNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            foreach (Edge edge in currentNode.edges)
            {
                Node neighbor = null;
                if(currentNode == edge.startNode)
                {
                    neighbor = edge.endNode;
                }
                else
                {
                    neighbor = edge.startNode;
                }
                //Debug.Log("Evaluating Neighbor: " + neighbor.position);

                if(closedSet.Contains(neighbor))
                {
                    //Debug.Log("Neighbor already evaluated");
                    continue;
                }

                float tentativeGScore = gScore[currentNode] + edge.distance;
                //Debug.Log("tentativeGScore for Neighbor: " + tentativeGScore);

                if(!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore[neighbor])
                {
                    //Debug.Log("Not a better path");
                    continue;
                }

                cameFrom[neighbor] = currentNode;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, endNode);
                //Debug.Log("Updated scores for Neighbor. gScore: " + gScore[neighbor] + ", fScore: " + fScore[neighbor]);
            }
        }
        Debug.Log("No path found");
        return null;
    }

    static List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node currentNode)
    {
        List<Node> path = new List<Node>();
        while (cameFrom.ContainsKey(currentNode))
        {
            path.Add(currentNode);
            currentNode = cameFrom[currentNode];
        }
        path.Add(currentNode); // Add the start node
        path.Reverse();
        return path;
    }

    static float HeuristicCostEstimate(Node a, Node b)
    {
        return Vector3.Distance(a.position, b.position);
    }
}
