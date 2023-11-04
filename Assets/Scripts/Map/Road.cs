using System.Collections.Generic;
using System.Xml;

public class Road
{
    public string name;
    public List<Node> Nodes = new List<Node>();
    public RoadType roadType;

    public Road(string name)
    {
        this.name = name;
    }
}