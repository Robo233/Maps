using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Changes the color of all child graphics (Image and Text) of a button when the button is pressed.
/// <summary>

public class ButtonChildColorChanger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector] public Color pressColor;
    [HideInInspector] public Color releaseColor;

    Button button; 

    Graphic[] childGraphics;

    [SerializeField] List<int> unaffectedChildrenIndexes;

     void Start()
    {
        button = GetComponent<Button>();
        childGraphics = GetComponentsInChildren<Graphic>(true); // This includes the button itself
        pressColor = GlobalConstants.ContrastForegroundColor;
        releaseColor = GlobalConstants.ForegroundColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button.interactable)
        {
            for (int i = 0; i < childGraphics.Length; i++)
            {
                if (unaffectedChildrenIndexes.Contains(i)) continue; // Skip unaffected children

                childGraphics[i].color = pressColor;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        for (int i = 0; i < childGraphics.Length; i++)
        {
            if (unaffectedChildrenIndexes.Contains(i)) continue; // Skip unaffected children

            childGraphics[i].color = releaseColor;
        }
    }
}