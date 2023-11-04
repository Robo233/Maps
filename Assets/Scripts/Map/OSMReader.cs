using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

/// <summary>
/// Creates the road system and handles the navigation between two points
/// </summary>

public class OSMReader : MonoBehaviour
{
    Dictionary<long, Vector3> nodes = new Dictionary<long, Vector3>();

    [SerializeField] RoadDrawer roadDrawer;
    [SerializeField] Navigation navigation;
    [SerializeField] PathFinder pathFinder;

    List<Road> roads = new List<Road>();

    List<Node> graph = new List<Node>();

    Dictionary<Vector3, Node> graphDict = new Dictionary<Vector3, Node>();

    public void ReadOSMData(string osmData, Place place)
    {
        XmlDocument xmlDoc = new XmlDocument();
        Debug.Log("osmData: " + osmData);
        xmlDoc.LoadXml(osmData);

        // Parse nodes
        XmlNodeList nodeList = xmlDoc.SelectNodes("/osm/node");
        foreach (XmlNode node in nodeList)
        {
            long id = long.Parse(node.Attributes["id"].Value);
            float lat = float.Parse(node.Attributes["lat"].Value) - ((place.upperRightLatitude + place.lowerLeftLatitude) / 2);
            float lon = float.Parse(node.Attributes["lon"].Value) - ((place.upperRightLongitude + place.lowerLeftLongitude) / 2);

            // Convert lat, lon to Unity world coordinates
            nodes[id] = new Vector3(lon, 0, lat);
        }

        // Parse ways
        XmlNodeList wayList = xmlDoc.SelectNodes("/osm/way");

        List<Edge> edges = new List<Edge>();

        foreach (XmlNode way in wayList)
        {
            Debug.Log("foreach (XmlNode way in wayList)");
            List<Vector3> wayPoints = new List<Vector3>();

            foreach (XmlNode nd in way.SelectNodes("nd"))
            {
                long refId = long.Parse(nd.Attributes["ref"].Value);
                wayPoints.Add(nodes[refId]);
            }

            for (int i = 0; i < wayPoints.Count; i++)
            {
                wayPoints[i] = new Vector3(wayPoints[i].x * GlobalConstants.scalingFactor, wayPoints[i].y, wayPoints[i].z * GlobalConstants.scalingFactor);
            }

            XmlNode buildingTag = way.SelectSingleNode("tag[@k='building']");
            XmlNode areaTag = way.SelectSingleNode("tag[@k='area']");
            XmlNode nameTag = way.SelectSingleNode("tag[@k='name']");
            
            string roadName = nameTag != null ? nameTag.Attributes["v"].Value : "Unnamed Way";

            if(buildingTag != null)
            {
                // Handle buildings...
            }
            else if(areaTag != null)
            {
                // TODO: Draw areas, in order to make places like Republicii better
            }
            else
            {
                Road road = new Road(roadName);
                List<Node> localRoadNodes = new List<Node>();

                Edge previousEdge = null;

                XmlNode typeNode = way.SelectSingleNode("tag[@k='highway']");
                if(typeNode == null) // Those typeNodes that can usually be found around buildings so they are useless in this case
                {
                    continue;
                }

                road.roadType = GlobalConstants.roadTypes[typeNode.Attributes["v"].Value];

                for (int i = 0; i < wayPoints.Count; i++)
                {
                    Node node = new Node();
                    Edge edge = null;
                
                    node.position = wayPoints[i];
                    if(!localRoadNodes.Contains(node))
                    {
                        localRoadNodes.Add(node);
                    }
                    if(!road.roadType.isWalkable)
                    {
                        continue;
                    }
                    if(i != wayPoints.Count - 1)
                    {
                        edge = new Edge();
                        edges.Add(edge);
                        edge.startNode = node;
                        edge.road = road;
                        node.edges.Add(edge);
                    }
                    if(previousEdge != null)
                    {
                        node.edges.Add(previousEdge);
                        previousEdge.endNode = node;
                        previousEdge.distance = Vector3.Distance(previousEdge.startNode.position, previousEdge.endNode.position);
                    }
                    previousEdge = edge;

                    if(!graphDict.ContainsKey(node.position))
                    {
                        graph.Add(node);
                        graphDict[node.position] = node;
                    }
                    else
                    {
                        graphDict[node.position].edges.AddRange(node.edges);
                    }

                } 

                road.Nodes = localRoadNodes;
                roads.Add(road);
            }
        
        }

        foreach(Edge edge in edges)
        {
            edge.startNode = graphDict[edge.startNode.position];
            edge.endNode = graphDict[edge.endNode.position];
        }

        
        pathFinder.graph = graph;
        navigation.edges = edges;
        Debug.Log("roadDrawer.DrawRoads(roads); " + roads.Count());
        roadDrawer.DrawRoads(roads);

    }

}