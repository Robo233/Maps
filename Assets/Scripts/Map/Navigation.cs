using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Navigation : MonoBehaviour
{
    GameObject DestinationRoute;
    GameObject DestinationLandmarkInstance;
    GameObject ClosestRoadSegmentToPlayer;
    GameObject CutRoadSegment;    
    GameObject CurvaturePathBetweenPlayerAndEdge;
    GameObject CurvaturePathBetweenDestinationAndEdge;
    [SerializeField] GameObject Player;
    [SerializeField] GameObject DestinationLandmark;

    List<GameObject> RoundedImages = new List<GameObject>();

    [SerializeField] Transform MapContainerCanvas;

    [SerializeField] Camera MainCamera;

    [SerializeField] Material DestinationRouteMaterial;

    [SerializeField] Sprite OnePixel;

    Vector3 DestinationPosition;
    Vector3 PreviousPositionOfThePlayer;

    [SerializeField] float distanceBetweenPlayerAndRouteUnderWhichPlayerIsOnCorrectRoad = 10;
    [SerializeField] float distanceBetweenPlayerAndDestinationUnderWhichRouteIsCompleted = 10;
    [SerializeField] float checkFrequency = 1;
    [SerializeField] float roadPositionYOffsetDestinationRoute = 0.0001f;
    [SerializeField] float destinationRouteWidth = 3;
    [SerializeField] float circleSize = 2;
    [SerializeField] float curvatureCircleInterval = 2.5f;

    bool wasDestinationSetBefore;

    Action OnArrivedToDestination;

    [SerializeField] RoadDrawer roadDrawer;
    [SerializeField] MonumentHandler monumentHandler;

    Node DestinationNode;
    Node DestinationNodeWhichIsPresentOnTheGraph;

    public List<Edge> edges;

    public List<Monument> monuments;

    Action ActionExecutedWhenPlayerArrivesToDestination;

    public void CreateRouteBetweenHitPositionAndPlayer()
    {
        DestinationNode = null;
        DestinationNodeWhichIsPresentOnTheGraph = null;
        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit)) // Handle inside this the buildings
        {
            Debug.Log(hit.collider.gameObject.tag);
            if(hit.collider.gameObject.tag == "Monument")
            {
                Debug.Log("Hit " + hit.collider.transform.root.name);
                Monument monument = monuments.Find(m => m.name == hit.collider.transform.root.name);
                Debug.Log("monument.name " + monument.name);
                DestinationPosition = GlobalConstants.ConvertWorldCoordinateToUnityCoordinate(monument.imagePosition);
                ActionExecutedWhenPlayerArrivesToDestination = () => monumentHandler.AskUserForCamera(monument);
            }
            else
            {
                DestinationPosition = hit.point;
                ActionExecutedWhenPlayerArrivesToDestination = () => Debug.Log("player arrived to chosen destination");
            }
            Node[] DestinationNodes = ClosestNodesToAPoint(DestinationPosition);
            DestinationNode = DestinationNodes[0];
            DestinationNodeWhichIsPresentOnTheGraph = DestinationNodes[1];
            CreateRouteBetweenDestinationAndThePlayer();
        }  
    }

    public void CreateRouteBetweenDestinationAndThePlayer()
    {
        Node[] ClosestNodesToThePlayer = ClosestNodesToAPoint(Player.transform.position);
        Node ClosestNodeToThePlayer = ClosestNodesToThePlayer[0];
        Node ClosestNodeToThePlayerWhichIsPresentOnTheGraph = ClosestNodesToThePlayer[1];

        List<Node> RouteNodes = PathFinder.AStarSearch(ClosestNodeToThePlayerWhichIsPresentOnTheGraph, DestinationNodeWhichIsPresentOnTheGraph);

       // if(isPointPresentOnLine(ClosestNodeToThePlayer.position, RouteNodes[RouteNodes.Count-2].position, ClosestNodeToThePlayerWhichIsPresentOnTheGraph.position))
      //  {
            RouteNodes.Remove(ClosestNodeToThePlayerWhichIsPresentOnTheGraph);
      //  }
        if(Vector3.Distance(Player.transform.position, DestinationPosition) <= distanceBetweenPlayerAndDestinationUnderWhichRouteIsCompleted)
        {
            ResetRoute();
            OnPlayerArrivedToDestination();
            return;
        }
        if(RouteNodes.Count > 3)
        {
            if(isPointPresentOnLine(DestinationNode.position, RouteNodes[RouteNodes.Count-2].position, DestinationNodeWhichIsPresentOnTheGraph.position))
            {
                RouteNodes.Remove(DestinationNodeWhichIsPresentOnTheGraph);
            }
        }
        else
        {
            RouteNodes.Remove(DestinationNodeWhichIsPresentOnTheGraph);
        }
        
        RouteNodes.Insert(0, ClosestNodeToThePlayer);
        RouteNodes.Add(DestinationNode);
        
        if(wasDestinationSetBefore)
        {
            ResetRoute();
        }

        CurvaturePathBetweenPlayerAndEdge = DrawCurvature(GetCurveBetweenPoints(Player.transform.position, ClosestNodeToThePlayer.position, curvatureCircleInterval));
        CurvaturePathBetweenDestinationAndEdge = DrawCurvature(GetCurveBetweenPoints(DestinationPosition, DestinationNode.position, curvatureCircleInterval));        
        DestinationRoute = roadDrawer.CreateRoad(RouteNodes, "DestinationRoute", DestinationRouteMaterial, true, roadPositionYOffsetDestinationRoute, destinationRouteWidth);
        DestinationLandmarkInstance = Instantiate(DestinationLandmark);
        DestinationLandmarkInstance.transform.position = DestinationPosition; 
        wasDestinationSetBefore = true;
        //DestinationRoute.transform.GetChild(0).gameObject.SetActive(false);
        StartCoroutine(UpdateRoute());
        
    }

    void ResetRoute()
    {
        Destroy(DestinationRoute);
        Destroy(DestinationLandmarkInstance);
        if(CurvaturePathBetweenPlayerAndEdge)
        {
            Destroy(CurvaturePathBetweenPlayerAndEdge);
        }
        Destroy(CurvaturePathBetweenDestinationAndEdge);
    }

    IEnumerator UpdateRoute()
    {
        yield return new WaitForSeconds(checkFrequency);
        if(PreviousPositionOfThePlayer == Player.transform.position)
        {
            yield return StartCoroutine(UpdateRoute());
            yield break;
        }
        PreviousPositionOfThePlayer = Player.transform.position;
        ClosestRoadSegmentToPlayer = GetClosestSegmentToPlayer();
        (Vector3 end1, Vector3 end2) = GetExtremes(ClosestRoadSegmentToPlayer);
        Destroy(ClosestRoadSegmentToPlayer);
        Vector3 projectedPoint = ProjectedPointOfAPointOnALine(Player.transform.position, end1, end2);
        if(CutRoadSegment)
        {
            Destroy(CutRoadSegment);
            
        }
        if(Vector3.Distance(Player.transform.position, projectedPoint) < distanceBetweenPlayerAndRouteUnderWhichPlayerIsOnCorrectRoad)
        {
            if(CurvaturePathBetweenPlayerAndEdge)
            {
                Destroy(CurvaturePathBetweenPlayerAndEdge);
            }
            CutRoadSegment = GameObject.CreatePrimitive(PrimitiveType.Quad);
            CutRoadSegment.name = "Quad";
            CutRoadSegment.transform.SetParent(DestinationRoute.transform);
            CutRoadSegment.transform.SetSiblingIndex(0);
            CutRoadSegment.transform.position = (projectedPoint + end1) / 2 + new Vector3(0, roadPositionYOffsetDestinationRoute, 0);
            Vector3 differenceVector = end1 - projectedPoint;
            CutRoadSegment.transform.right = differenceVector;
            CutRoadSegment.transform.localScale = new Vector3(differenceVector.magnitude, destinationRouteWidth, 1);
            CutRoadSegment.transform.Rotate(90, 0, 0);
            CutRoadSegment.GetComponent<Renderer>().material = DestinationRouteMaterial;
            StartCoroutine(UpdateRoute());
        }
        else
        {
            CreateRouteBetweenDestinationAndThePlayer();
        }
        
    }


    GameObject GetClosestSegmentToPlayer()
    {
        GameObject RoadSegment1 = DestinationRoute.transform.GetChild(0).gameObject;
        GameObject RoadSegment2;
        if(DestinationRoute.transform.childCount > 1)
        {
            RoadSegment2 = DestinationRoute.transform.GetChild(1).gameObject;
        }
        else
        {
            return RoadSegment1;
        }
        (Vector3 RoadSegment1End1, Vector3 RoadSegment1End2) = GetExtremes(RoadSegment1);
        (Vector3 RoadSegment2End1, Vector3 RoadSegment2End2) = GetExtremes(RoadSegment2);
        if(distanceBetweenPointAndLine(Player.transform.position, RoadSegment1End1, RoadSegment1End2  ) < distanceBetweenPointAndLine(Player.transform.position, RoadSegment2End1, RoadSegment2End2) )
        {
            return RoadSegment1;
        }
        return RoadSegment2;
    }

    float distanceBetweenPointAndLine(Vector3 point, Vector3 A, Vector3 B)
    {
        return Vector3.Distance(point, ProjectedPointOfAPointOnALine(point, A, B));        
    }

    Vector3 ProjectedPointOfAPointOnALine(Vector3 point, Vector3 A, Vector3 B)
    {
        Vector3 AB = B - A;
        return A + Mathf.Clamp01(Vector3.Dot(point - A, AB) / Vector3.Dot(AB, AB)) * AB;
    } 


    public (Vector3 rightmost, Vector3 leftmost) GetExtremes(GameObject obj)
    {
        Vector3 position = obj.transform.position;
        Quaternion rotation = obj.transform.rotation;
        Vector3 scale = obj.transform.localScale;
        float halfExtentX = scale.x * 0.5f;
    
        Vector3 localRightmost = new Vector3(halfExtentX, 0, 0);
        Vector3 localLeftmost = new Vector3(-halfExtentX, 0, 0);

        Vector3 worldRightmost = position + rotation * localRightmost;
        Vector3 worldLeftmost = position + rotation * localLeftmost;

        return (worldRightmost, worldLeftmost);
    }

    int GetChildIndex(GameObject child)
    {
        Transform parentTransform = child.transform.parent;

        for (int i = 0; i < parentTransform.childCount; i++)
        {
            if (parentTransform.GetChild(i) == child.transform)
            {
                return i;
            }
        }

        return -1;
    }

    GameObject DrawCurvature(Vector3[] Curvature)
    {
        GameObject CurvatureParent = new GameObject("CurvatureParent");
        CurvatureParent.transform.SetParent(MapContainerCanvas);        
        foreach(Vector3 position in Curvature)
        {
            GameObject RoundedImage = new GameObject("Circle");
            RoundedImage.transform.SetParent(CurvatureParent.transform);
            RoundedImages.Add(RoundedImage);
            Image imageComp = RoundedImage.AddComponent<Image>();            
            RoundedImage.AddComponent<ImageWithRoundedCorners>().radius = circleSize/2;
            RoundedImage.transform.localEulerAngles = new Vector3(90, 0, 0);
            RoundedImage.transform.position = position;
            RectTransform rectTransform = imageComp.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(circleSize, circleSize);
            
        }
        return CurvatureParent;
    }

    bool isPointPresentOnLine(Vector3 point, Vector3 A, Vector3 B)
    {   
        Vector3 AP = point - A;
        Vector3 AB = B - A;

        float dotProduct = Vector3.Dot(AP, AB);
        float squaredMagnitude = AB.sqrMagnitude;

        if (dotProduct < 0 || dotProduct > squaredMagnitude)
        {
            return false;
        }

        Vector3 crossProduct = Vector3.Cross(AP, AB);
        float epsilon = 1e-5f;

        if (crossProduct.sqrMagnitude < epsilon)
        {
            return true;
        }

        return false;
    }

    Node[] ClosestNodesToAPoint(Vector3 point)
    {
        float minDistance = float.MaxValue;
        Edge closestEdge = null;
        Vector3 ClosestPoint = Vector3.zero;

        foreach (Edge edge in edges)
        {
            Vector3 A = edge.startNode.position;
            Vector3 B = edge.endNode.position;
            Vector3 AB = B - A;

            float t = Vector3.Dot(point - A, AB) / Vector3.Dot(AB, AB);
        
            // Clamp t to [0, 1] to ensure it's within the line segment
            t = Mathf.Clamp01(t);
            Vector3 projectedPoint = A + t * AB;
            float currentDistance = Vector3.Distance(point, projectedPoint);

            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                closestEdge = edge;
                ClosestPoint = projectedPoint;
            }
        }

        Node closestNodeWhichIsPresentOnTheGraph;
        float distanceBetweenNewNodeAndEndNode = Vector3.Distance(closestEdge.endNode.position, ClosestPoint);
        float distanceBetweenNewNodeAndStartNode = Vector3.Distance(closestEdge.startNode.position, ClosestPoint);
        closestNodeWhichIsPresentOnTheGraph = distanceBetweenNewNodeAndEndNode < distanceBetweenNewNodeAndStartNode ? closestEdge.endNode : closestEdge.startNode;
        Node closestNode = new Node(ClosestPoint);
        Debug.Log("closestEdge: " + closestEdge.road.name);
        return new Node[] { closestNode, closestNodeWhichIsPresentOnTheGraph };
    }

    public Vector3[] GetCurveBetweenPoints(Vector3 StartingPosition, Vector3 EndingPosition, float interval)
    {
        List<Vector3> positions = new List<Vector3>();

        Vector3 D = EndingPosition - StartingPosition;
        float totalDistance = D.magnitude;
    
        Vector3 U = D.normalized;

        // A coefficient to ensure consistent curvature regardless of the distance
        float curvatureCoefficient = totalDistance / (2 * Mathf.PI); // Here, 2 * Mathf.PI is a heuristic value, you can adjust this coefficient to suit your requirements.

        Vector3 P = new Vector3(-U.z, 0, U.x);  // This is a vector perpendicular to D in the XZ plane.

        float t = 0;
        float accumulatedDistance = 0;
        float deltaT = 0.01f;  // Small step for t

        Vector3 previousPosition = StartingPosition;

        while (t <= 1)
        {
            float displacement = curvatureCoefficient * Mathf.Sin(Mathf.PI * t); // We're multiplying by the curvatureCoefficient to ensure consistent curvature
            Vector3 currentPosition = StartingPosition + t * D + displacement * P;

            accumulatedDistance += Vector3.Distance(previousPosition, currentPosition);

            if (accumulatedDistance >= interval)
            {
                positions.Add(currentPosition);
                accumulatedDistance = 0;  // Reset accumulated distance
            }

            previousPosition = currentPosition;
            t += deltaT;
        }

        return positions.ToArray();
    }

    void OnPlayerArrivedToDestination()
    {
        StopAllCoroutines();
        Debug.Log("Player arrived");
        ActionExecutedWhenPlayerArrivesToDestination.Invoke();
    }
    
}