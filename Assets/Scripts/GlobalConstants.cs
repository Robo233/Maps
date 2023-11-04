using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalConstants : MonoBehaviour
{
    [SerializeField] Material MotorwayMaterial;
    [SerializeField] Material TrunkMaterial;
    [SerializeField] Material PrimaryMaterial;
    [SerializeField] Material SecondaryMaterial;
    [SerializeField] Material TertiaryMaterial;
    [SerializeField] Material UnclassifiedMaterial;
    [SerializeField] Material ResidentialMaterial;
    [SerializeField] Material ServiceMaterial;
    [SerializeField] Material TrackMaterial;
    [SerializeField] Material FootwayMaterial;
    [SerializeField] Material BridlewayMaterial;
    [SerializeField] Material CyclewayMaterial;
    [SerializeField] Material PathMaterial;
    [SerializeField] Material BuswayMaterial;
    [SerializeField] Material RacewayMaterial;
    [SerializeField] Material PedestrianMaterial;
    [SerializeField] Material LivingStreetMaterial;
    [SerializeField] Material StepsMaterial;
    [SerializeField] Material CorridorMaterial;
    [SerializeField] Material ConstructionMaterial;
    [SerializeField] Material ProposedMaterial;
    [SerializeField] Material DisusedMaterial;
    [SerializeField] Material AbandonedMaterial;

    public static Color BackgroundColor = Color.black;
    public static Color ForegroundColor = new Color(207/255f, 207/255f, 207/255f, 1);
    public static Color ContrastForegroundColor = new Color(185/255f, 185/255f, 185/255f, 1);

    public static float scalingFactor = 100000;
    public float upperRightLongitude;

    public static string placeURL;

    public static string placePath;

    public static bool isVuforiaInitialized;

    public static Dictionary<string, RoadType> roadTypes;

    public static Place place;

    void Awake()
    {
        placePath = Application.persistentDataPath + "/Places/";

        roadTypes = new Dictionary<string, RoadType>() 
        {
            { "motorway", new RoadType(MotorwayMaterial) },
            { "motorway_link", new RoadType(MotorwayMaterial) },
    
            { "trunk", new RoadType(TrunkMaterial) },
            { "trunk_link", new RoadType(TrunkMaterial) },
    
            { "primary", new RoadType(PrimaryMaterial, false, 5, true) },
            { "primary_link", new RoadType(PrimaryMaterial, false, 5, true) },
    
            { "secondary", new RoadType(SecondaryMaterial) },
            { "secondary_link", new RoadType(SecondaryMaterial) },
    
            { "tertiary", new RoadType(TertiaryMaterial, false, 5, true) },
            { "tertiary_link", new RoadType(TertiaryMaterial, false, 5, true) },

            { "unclassified", new RoadType(UnclassifiedMaterial) },
            { "residential", new RoadType(ResidentialMaterial) },
    
            { "service", new RoadType(ServiceMaterial) },
            { "track", new RoadType(TrackMaterial) },
            { "footway", new RoadType(FootwayMaterial) },
            { "bridleway", new RoadType(BridlewayMaterial) },
            { "cycleway", new RoadType(CyclewayMaterial) },
            { "path", new RoadType(PathMaterial) },
    
            { "busway", new RoadType(BuswayMaterial) },
            { "raceway", new RoadType(RacewayMaterial) },
            { "pedestrian", new RoadType(PedestrianMaterial, true, 3) },
            { "living_street", new RoadType(LivingStreetMaterial) },
            { "steps", new RoadType(StepsMaterial) },
            { "corridor", new RoadType(CorridorMaterial) },
    
            { "construction", new RoadType(ConstructionMaterial) },
            { "proposed", new RoadType(ProposedMaterial) },
            { "disused", new RoadType(DisusedMaterial) },
            { "abandoned", new RoadType(AbandonedMaterial) }
        };
        
    }

    public static Vector3 ConvertWorldCoordinateToUnityCoordinate(Vector3 WorldPosition)
    {
        return new Vector3
        (
            (WorldPosition.x - ((place.upperRightLongitude + place.lowerLeftLongitude) / 2))  * scalingFactor,
            0,
            (WorldPosition.z - ((place.upperRightLatitude + place.lowerLeftLatitude) / 2 ) )  * scalingFactor
        );
    }
}
