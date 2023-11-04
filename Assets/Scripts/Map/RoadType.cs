using UnityEngine;

public class RoadType
{
    public Material material;

    public float width = 1;
    public float roadPositionYOffset;

    public bool isWalkable;
    public bool shouldItBePlacedLower; // It's used in order to display walkable roads over roads for cars

    public RoadType(Material material, bool isWalkable = true, float width = 0, bool shouldItBePlacedLower = false)
    {
        this.material = material;
        this.isWalkable = isWalkable;
        if(width != 0)
        {
            this.width = width;
        }
        
        this.shouldItBePlacedLower = shouldItBePlacedLower;

    }

}
