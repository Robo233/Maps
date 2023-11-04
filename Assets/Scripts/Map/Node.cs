using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public GameObject DestinationRoadSegment;

    public Vector3  position;

    public List<Edge> edges = new List<Edge>(); // TODO: check if it is more optimal to put this in the constructors

    public Node() {}

    public Node(Vector3  position)
    {
        this.position = position;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Node))
            return false;

        Node otherNode = obj as Node;
        return position == otherNode.position;
    }

    public override int GetHashCode()
    {
        return position.GetHashCode();
    }

}