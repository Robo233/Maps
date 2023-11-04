using UnityEngine;

/// <summary>
/// It rotates the LoadingImages
/// </summary>

public class LoadingRotation: MonoBehaviour
{
    RectTransform rectTransform;

    float rotateSpeed = 0.25f;
    float angle = 22.5f;
    float timePassed;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void FixedUpdate()
    {
        timePassed += Time.deltaTime;
        if(timePassed > rotateSpeed)
        {
            rectTransform.localEulerAngles = new Vector3(rectTransform.localEulerAngles.x, rectTransform.localEulerAngles.y, rectTransform.localEulerAngles.z - angle);
            timePassed = 0;
        }
        
    }

}