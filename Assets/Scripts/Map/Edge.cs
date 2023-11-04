public class Edge
{
    public Road road;
    public Node startNode;
    public Node endNode;
    public float distance;

    public override bool Equals(object obj)
    {
        if (obj == null || !(obj is Edge))
            return false;

        Edge otherEdge = obj as Edge;
        return startNode.Equals(otherEdge.startNode) && endNode.Equals(otherEdge.endNode);
    }

    public override int GetHashCode()
    {
        int hashStart = startNode != null ? startNode.GetHashCode() : 0;
        int hashEnd = endNode != null ? endNode.GetHashCode() : 0;

        // Combine the hash codes of the startNode and endNode
        return hashStart ^ hashEnd;
    }

}
