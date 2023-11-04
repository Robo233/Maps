using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Assets.SimpleZip;

public class PlacesListGenerator : MonoBehaviour
{
    [SerializeField] GameObject RoundedButton;
    [SerializeField] GameObject MainCanvas;

    [SerializeField] Transform GridContentPlacesList;
    GameObject Region;

    List<GameObject> PlaceButtons = new List<GameObject>();

    int placeCount;

    [SerializeField] float placeButtonDistanceFromSide = 208.33f;

    [SerializeField] OSMReader osmReader;
    [SerializeField] MapGenerator mapGenerator;
    [SerializeField] MapMovement mapMovement;
    [SerializeField] MonumentHandler monumentHandler;
    [SerializeField] Navigation navigation;

    List<Place> places = new List<Place>();

    void Start()
    {
        GeneratePlacesList();
    }

    void GeneratePlacesList()
    {
        StartCoroutine(GetPlaces());
    }

    IEnumerator GetPlaces()
    {
        yield return StartCoroutine(FileLoader.DownloadTextCoroutine(GlobalConstants.placeURL, GlobalConstants.placePath + "Places.json", true, (text) =>
        {
            if(text != null)
            {
                Debug.Log(text);
                var jObject = JObject.Parse(text);
                var placesJArray = jObject["places"] as JArray;

                foreach (var item in placesJArray)
                {
                    places.Add(new Place( item["name"].ToString() ) );
                }
                placeCount = places.Count;
                PreloadPlaceButtons(placeCount);
            }
            else
            {
                Debug.Log("Internet is not working" + GlobalConstants.placePath + "Places.json");
                StartCoroutine(GetPlaces());
            }
        }));
    }

    IEnumerator DownloadPlaceImage(int i)
    {
        if(i < placeCount)
        {
            yield return StartCoroutine(FileLoader.DownloadTextureCoroutine(GlobalConstants.placeURL + places[i].name + "/Image.jpg", GlobalConstants.placePath + places[i].name + "/Image.jpg", true, (texture) => 
            {
                if(texture)
                {
                    PlaceButtons[i].transform.GetChild(4).transform.GetChild(1).GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), new Vector2(1,1));     
                    PlaceButtons[i].GetComponent<Button>().onClick.AddListener(() =>
                    {
                        OpenPlace(places[i]);
                    });
                    StartCoroutine(DownloadPlaceImage(i+1));
                }
                else
                {
                    StartCoroutine(DownloadPlaceImage(i));
                }
            }));
        }
    }

    void PreloadPlaceButtons(int numberOfPlaces)
    {
        for(int i=0;i<numberOfPlaces;i++)
        {
            if(i%2 == 0)
            {
                Region = new GameObject("Region " + i/2);
                Region.transform.SetParent(GridContentPlacesList, false);
                Region.AddComponent<RectTransform>().sizeDelta = new Vector2(0, -130);
                
            }
            GameObject PlaceButtonInstance = Instantiate(RoundedButton);
            PlaceButtonInstance.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0); // LoadingImageRoundedButton
            PlaceButtonInstance.GetComponent<Image>().color = GlobalConstants.ForegroundColor; // Before the placeImage is loaded, the button would be white so it is set to the ForegroundColor
            PlaceButtonInstance.transform.SetParent(Region.transform);
            placeButtonDistanceFromSide *= -1;
            PlaceButtonInstance.GetComponent<RectTransform>().anchoredPosition  = new Vector2(placeButtonDistanceFromSide, 0);
            PlaceButtons.Add(PlaceButtonInstance);
        }     
        StartCoroutine(DownloadPlaceImage(0));   
    }

    void OpenPlace(Place place)
    {
        MainCanvas.SetActive(false);
        GlobalConstants.place = place;
        StartCoroutine(DownloadPlace(place));
    }

    IEnumerator DownloadPlace(Place place)
    {
        yield return StartCoroutine(FileLoader.DownloadZipCoroutine(GlobalConstants.placeURL + "/" + place.name + "/Map.zip", GlobalConstants.placePath + place.name + "/Map.zip", true, (isDownloadSuccessful) =>
        {
            if(isDownloadSuccessful)
            {
                Zip.DecompressArchive(GlobalConstants.placePath + place.name + "/Map.zip", GlobalConstants.placePath + place.name);

                JObject jsonObj = JObject.Parse(Encoding.UTF8.GetString(File.ReadAllBytes(GlobalConstants.placePath + place.name + "/Location.json")));
                place.upperRightLatitude = jsonObj["coordinates"]["upperRightLatitude"].Value<float>();
                place.upperRightLongitude = jsonObj["coordinates"]["upperRightLongitude"].Value<float>();
                place.lowerLeftLatitude = jsonObj["coordinates"]["lowerLeftLatitude"].Value<float>();
                place.lowerLeftLongitude = jsonObj["coordinates"]["lowerLeftLongitude"].Value<float>();

                osmReader.ReadOSMData(Encoding.UTF8.GetString(File.ReadAllBytes(GlobalConstants.placePath + place.name + "/Map.xml")), place);
                StartCoroutine(mapGenerator.DownloadMapImages(place));
                mapMovement.CreateTouchDetector(place);

                JArray monumentsArray = JArray.Parse(jsonObj["monuments"].ToString());

                List<Monument> monuments = new List<Monument>();

                foreach (JObject monumentObj in monumentsArray)
                {
                    string name = monumentObj["name"].ToString();

                    Vector3 position = new Vector3(monumentObj["position"]["lon"].Value<float>(), 0, monumentObj["position"]["lat"].Value<float>());
                    Vector3 imagePosition = new Vector3(monumentObj["imagePosition"]["lon"].Value<float>(), 0, monumentObj["imagePosition"]["lat"].Value<float>());

                    Monument monument = new Monument(name, position, imagePosition, place);
                    monuments.Add(monument);
                }

                monumentHandler.StartMonumentDownload(monuments);
                navigation.monuments = monuments;
              
            }
        }));
    }

}