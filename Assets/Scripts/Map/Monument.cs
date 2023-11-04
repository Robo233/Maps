using UnityEngine;

public class Monument
{
    public string name;

    public Vector3 position;
    public Vector3 imagePosition;

    public Place place;

    public Monument(string name, Vector3 position, Vector3 imagePosition, Place place)
    {
        this.name = name;
        this.position = position;
        this.imagePosition = imagePosition;
        this.place = place;
    }
}
