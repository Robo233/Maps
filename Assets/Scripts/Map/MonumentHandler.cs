using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class MonumentHandler : MonoBehaviour
{
    [SerializeField] GameObject ScanThisImagePanel;

    public void StartMonumentDownload(List<Monument> monuments)
    {
        foreach(Monument monument in monuments)
        {
            StartCoroutine(DownloadMonument(monument));
        }
    }

    IEnumerator DownloadMonument(Monument monument)
    {
        string monumentFile = "/" + monument.place.name + "/Monuments/" + monument.name + "/" + "Model.glb";
        yield return StartCoroutine(FileLoader.DownloadModelCoroutine( GlobalConstants.placeURL + monumentFile, GlobalConstants.placePath + monumentFile, "", true, (GameObject MonumentModel) =>
        {
            Debug.Log("GlobalConstants.placePath + monumentFile: " + GlobalConstants.placePath + monumentFile);
            if(MonumentModel)
            {
                MonumentModel.transform.position = GlobalConstants.ConvertWorldCoordinateToUnityCoordinate(monument.position);
                MonumentModel.name = monument.name;
                Debug.Log("children: " + MonumentModel.transform.childCount);
                foreach (Transform child in MonumentModel.transform.GetComponentsInChildren<Transform>())
                {
                    if(child.childCount == 0)
                    {
                        child.gameObject.AddComponent<MeshCollider>(); // It's needed to detect the gameObject using raycast
                        child.gameObject.tag = "Monument";
                    }

                }        
            }
            
        }));
    }

    public void AskUserForCamera(Monument monument)
    {
        Debug.Log("Arrived to " + monument.name);
        StartCoroutine(InitializeVuforia());
    }

    IEnumerator InitializeVuforia()
    {
        if(!GlobalConstants.isVuforiaInitialized)
        {
            yield return StartCoroutine(VuforiaInitializationCoroutine());
            GlobalConstants.isVuforiaInitialized = true;
        }
        VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaInitialized;
        
    }

    IEnumerator VuforiaInitializationCoroutine()
    {
        yield return null;
        VuforiaApplication.Instance.Initialize(); 

    }

    void OnVuforiaInitialized(VuforiaInitError error)
    {
        ScanThisImagePanel.SetActive(true);
        
    }

    
    

}