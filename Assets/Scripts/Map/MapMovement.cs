using System.Collections;
using UnityEngine;

public class MapMovement : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float inertiaDuration = 0.5f;
    [SerializeField] float inertiaSpeedFactor = 2.0f;
    [SerializeField] float pinchZoomSpeed = 2.0f;
    [SerializeField] float minimumCameraPositionY;
    [SerializeField] float maximumCameraPositionY;
    float currentInertiaTime;
    float lastPinchDistance;
    float touchStartTime;
    float cameraPositionY;
    float mapWidth;
    float mapHeight;

    [SerializeField] bool wasStationary;

    Vector2 lastTouchDeltaPosition;

    [SerializeField] Navigation navigation;

    void Awake()
    {
        cameraPositionY = transform.position.y;
    }

    public void CreateTouchDetector(Place place)
    {
        mapWidth = (place.upperRightLongitude - place.lowerLeftLongitude) * GlobalConstants.scalingFactor;
        mapHeight = (place.upperRightLatitude - place.lowerLeftLatitude) * GlobalConstants.scalingFactor;

        GameObject TouchDetectorQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        TouchDetectorQuad.name = "TouchDetectorQuad";
        TouchDetectorQuad.transform.localScale = new Vector3(mapWidth, mapHeight, 1);
        TouchDetectorQuad.transform.localEulerAngles = new Vector3(90, 0, 0);
        TouchDetectorQuad.transform.position = new Vector3(0, 0.01f, 0);
        Destroy(TouchDetectorQuad.GetComponent<MeshRenderer>());
    }

    void Update()
    {
        if(Input.touchCount == 2) // pinching
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float previousTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = previousTouchDeltaMag - touchDeltaMag;

            transform.Translate(0, deltaMagnitudeDiff * pinchZoomSpeed, 0);

            if(touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                lastPinchDistance = touchDeltaMag;
            }

            float pinchDistanceDelta = touchDeltaMag - lastPinchDistance;
            Vector3 pinchMidPoint = (touchZero.position + touchOne.position) * 0.5f;
            Vector3 movementDirection = (Camera.main.ScreenToWorldPoint(pinchMidPoint) - transform.position).normalized;

            transform.Translate(movementDirection * pinchDistanceDelta * pinchZoomSpeed * Time.deltaTime, Space.World);
            RepositionCamera(minimumCameraPositionY, maximumCameraPositionY);
            cameraPositionY = transform.position.y;
        }
        else if(Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            switch(touch.phase)
            {
                case TouchPhase.Began:
                    touchStartTime = Time.time;
                    StartCoroutine(SetCurrentInertiaTimeToZeroAfterDelay()); // It has to be set back to 0 after a delay, because otherwise the StartSearch of navigation won't be called correctly
                    break;
                case TouchPhase.Moved:
                    wasStationary = false;
                    Vector2 TouchDeltaPosition = touch.deltaPosition;

                    transform.Translate(-TouchDeltaPosition.x * speed, 0, -TouchDeltaPosition.y * speed);
                    lastTouchDeltaPosition = TouchDeltaPosition;
                    currentInertiaTime = inertiaDuration;
                    break;
                case TouchPhase.Ended:
                    float touchDuration = Time.time - touchStartTime;
                    if (touchDuration < 0.2f && touch.deltaPosition.magnitude < 10.0f && currentInertiaTime == 0)
                    {
                        navigation.CreateRouteBetweenHitPositionAndPlayer();
                    }
                    
                    break;
                case TouchPhase.Stationary:
                    wasStationary = true;
                    break;
            }
            RepositionCamera(cameraPositionY, cameraPositionY);
        }
        else if(currentInertiaTime > 0 && !wasStationary) // Apply inertia
        {
            currentInertiaTime -= Time.deltaTime;
            float lerpFactor = currentInertiaTime / inertiaDuration;

            Vector2 inertia = Vector2.Lerp(Vector2.zero, lastTouchDeltaPosition, lerpFactor);
            transform.Translate(-inertia.x * speed * inertiaSpeedFactor, 0, -inertia.y * speed * inertiaSpeedFactor);
            RepositionCamera(cameraPositionY, cameraPositionY);
        }
        

    }

    void RepositionCamera(float minimumCameraPositionY, float maximumCameraPositionY)
    {
        transform.position = new Vector3
        (
            Mathf.Clamp(transform.position.x, -mapWidth/2, mapWidth/2),
            Mathf.Clamp(transform.position.y, minimumCameraPositionY, maximumCameraPositionY),
            Mathf.Clamp(transform.position.z, -mapHeight/2, mapHeight/2)
        );
    }

    IEnumerator SetCurrentInertiaTimeToZeroAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        currentInertiaTime = 0;
    }
}