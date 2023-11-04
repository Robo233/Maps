using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml;
using UnityEngine;

public class RoadDrawer : MonoBehaviour
{
    [SerializeField] float initialRoadPositionYOffset = 0.0001f;
    float roadPositionYOffsetIncrement = 0.0001f;
    float distanceBetweenMapAndRoad = 0.001f;
    float distanceBetweenMapAndRoadLow = 0.0009f; // using a lower value can caue flickering

    Vector3 PreviousTopLeftCorner;
    Vector3 PreviousTopRightCorner;

    void Awake()
    {
        ModifyRoadTypeOffsetValues(); // In order to prevent flickering when multiple roads overlap each other
        
    }

    void ModifyRoadTypeOffsetValues()
    {
        if(GlobalConstants.roadTypes != null) // throws exception sometimes
        {
            foreach (KeyValuePair<string, RoadType> entry in GlobalConstants.roadTypes)
            {
                entry.Value.roadPositionYOffset = initialRoadPositionYOffset;
                initialRoadPositionYOffset += roadPositionYOffsetIncrement;
            }
        }

    }

    public void DrawRoads(List<Road> roads)
    {
        foreach (Road road in roads)
        {
            if(road.roadType == null) // Sometimes the roadType is null and causes exception
            {
                continue;
            }
            CreateRoad(road.Nodes, road.name, road.roadType.material, road.roadType.shouldItBePlacedLower, road.roadType.roadPositionYOffset, road.roadType.width );
        }        
    }

    public GameObject CreateRoad(List<Node> nodes, string roadName, Material material, bool shouldItBePlacedLower, float roadPositionYOffset, float width)
    {
        GameObject RoadObject = new GameObject(roadName);
        for (int i = 0; i < nodes.Count - 1; i++)  // -1 to avoid accessing out of bounds
        {
            GameObject RoadSegment = GameObject.CreatePrimitive(PrimitiveType.Quad);
            RoadSegment.name = "RoadSegment";
            float roadPositionY = (shouldItBePlacedLower ? distanceBetweenMapAndRoadLow : distanceBetweenMapAndRoad) + roadPositionYOffset;
            Vector3 startPos = nodes[i].position;
            Vector3 endPos = nodes[i + 1].position;
            RoadSegment.transform.position = (startPos + endPos) / 2 + new Vector3(0, roadPositionY, 0);

            Vector3 differenceVector = endPos - startPos;
            RoadSegment.transform.right = differenceVector;
            RoadSegment.transform.localScale = new Vector3(differenceVector.magnitude, width, 1);

            RoadSegment.transform.Rotate(90, 0, 0);
            RoadSegment.GetComponent<Renderer>().material = material;

            Vector3 midpoint = (startPos + endPos) / 2;
            Vector3 direction = (endPos - startPos).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
            Vector3 offsetPosition = perpendicular * width / 2.0f;                

            // Here is created a triangle to make the roads smoother when they are curved
            GameObject triangle = null;
            if(i>0)
            {
                Vector3 BottomLeftCorner = midpoint - offsetPosition - direction * differenceVector.magnitude / 2;
                Vector3 BottomRightCorner = midpoint + offsetPosition - direction * differenceVector.magnitude / 2;

                Mesh mesh = new Mesh();

                if(Vector3.Distance(RoadSegment.transform.position, BottomLeftCorner) < Vector3.Distance(RoadSegment.transform.position, PreviousTopLeftCorner) )
                {
                    mesh.vertices = new Vector3[3] {nodes[i].position, BottomLeftCorner, PreviousTopLeftCorner};
                    mesh.triangles = new int[3]{ 0, 1, 2};
                }
                else
                {
                    mesh.vertices = new Vector3[3] {nodes[i].position, BottomRightCorner, PreviousTopRightCorner};
                    mesh.triangles = new int[3]{ 0, 2, 1};
                }

                mesh.RecalculateNormals();

                triangle = new GameObject("Triangle", typeof(MeshFilter), typeof(MeshRenderer));
                triangle.GetComponent<MeshFilter>().mesh = mesh;
                triangle.GetComponent<MeshRenderer>().material = material;  
                triangle.transform.position = new Vector3(0, roadPositionY, 0);                   
                    
            }
            if(i != nodes.Count - 2)
            {
                Vector3 TopLeftCorner = midpoint - offsetPosition + direction * differenceVector.magnitude / 2;
                Vector3 TopRightCorner = midpoint + offsetPosition + direction * differenceVector.magnitude / 2;
                PreviousTopLeftCorner = TopLeftCorner;   
                PreviousTopRightCorner = TopRightCorner;                 
            }

            if (triangle)
            {
                MeshFilter roadSegmentFilter = RoadSegment.GetComponent<MeshFilter>();
                MeshFilter triangleFilter = triangle.GetComponent<MeshFilter>();

                CombineInstance[] combine = new CombineInstance[2];

                combine[0].mesh = roadSegmentFilter.mesh;
                combine[0].transform = Matrix4x4.identity;  // RoadSegment as reference

                // Calculate the transform of the triangle relative to the RoadSegment
                Matrix4x4 triangleRelativeTransform = RoadSegment.transform.worldToLocalMatrix * triangle.transform.localToWorldMatrix;

                combine[1].mesh = triangleFilter.mesh;
                combine[1].transform = triangleRelativeTransform;

                Mesh combinedMesh = new Mesh();
                combinedMesh.CombineMeshes(combine, true);  // 'true' will merge submeshes
                combinedMesh.RecalculateNormals();

                roadSegmentFilter.mesh = combinedMesh;

                Destroy(triangle);
            }

            RoadSegment.transform.SetParent(RoadObject.transform);

        }

        return RoadObject;
    }

}