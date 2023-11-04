using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleZip;

/// <summary>
/// Loads the images, which contain the buildings of the map
/// </summary>

public class MapGenerator : MonoBehaviour
{
    [SerializeField] Transform MapContainerCanvas;

    [SerializeField] Color MapBaseColor;

    Vector3 MapOffset = new Vector3(191.06f, 0, 72.24f); // It is not known from where this value comes from. For 100 images: 98.8f, 0, 74.3f

    int numberOfImages = 256;

    string path;

    public Place place;

    void Awake()
    {
        path = Application.persistentDataPath + "/Places/";
    }

    public IEnumerator DownloadMapImages(Place place)
    {
        this.place = place;
        yield return StartCoroutine(FileLoader.DownloadZipCoroutine(GlobalConstants.placeURL + "/" + place.name + "/MapImages.zip", path + place.name + "/MapImages.zip", true, (isDownloadSuccessful) =>
        {
            if(isDownloadSuccessful)
            {
                Zip.DecompressArchive(path + place.name + "/MapImages.zip", path + place.name + "/MapImages");
                LoadBuildings();
            }
        }));
    }

    void LoadBuildings()
    {
        int numberOfImagesSquareRoot = (int)Math.Sqrt(numberOfImages);

        float widthIncrement = ((place.upperRightLongitude - place.lowerLeftLongitude) * GlobalConstants.scalingFactor) / numberOfImagesSquareRoot;
        float heightIncrement = ((place.upperRightLatitude - place.lowerLeftLatitude) * GlobalConstants.scalingFactor) / numberOfImagesSquareRoot;
        int imageCount = 0;
        for (float i = place.lowerLeftLongitude * GlobalConstants.scalingFactor; i < place.upperRightLongitude * GlobalConstants.scalingFactor; i += widthIncrement)
        {
            for (float j = place.lowerLeftLatitude * GlobalConstants.scalingFactor; j < place.upperRightLatitude * GlobalConstants.scalingFactor; j += heightIncrement)
            {
                GameObject BuildingImage = new GameObject("BuildingImage");
                BuildingImage.transform.SetParent(MapContainerCanvas);
                BuildingImage.transform.localEulerAngles = Vector3.zero;
                Image imageComp = BuildingImage.AddComponent<Image>();
                RectTransform rectTransform = imageComp.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(widthIncrement, heightIncrement);
                imageCount++;
                Texture2D texture = new Texture2D(1, 1);
                string imagePath = path + place.name + "/MapImages" + "/output_map_tile_" + imageCount + ".png";
                if(File.Exists(imagePath))
                {
                    texture.LoadImage(File.ReadAllBytes(imagePath));
                    Sprite mapSprite = Sprite.Create(texture, new Rect(0,0,texture.width,texture.height), new Vector2(1,1));
                    imageComp.sprite = mapSprite;
                }
                else
                {
                    imageComp.color = MapBaseColor;
                }
            
                rectTransform.anchoredPosition = new Vector2( (i/GlobalConstants.scalingFactor - ((place.upperRightLongitude + place.lowerLeftLongitude) / 2)) * GlobalConstants.scalingFactor , (j/GlobalConstants.scalingFactor - ((place.upperRightLatitude + place.lowerLeftLatitude) / 2)) * GlobalConstants.scalingFactor );
            }
        }
        MapContainerCanvas.transform.position = MapOffset;
    }
        
}