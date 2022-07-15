using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class VectorEditor : MonoBehaviour
{
    public float padding;                   // distance between child elements
    RectTransform rectTransform;            // transform of parent
    [HideInInspector]
    public List<RectTransform> children;    // list of children

    private void OnValidate()
    {
        GetChildren();
        PlaceChildren();
    }

    // Start is called before the first frame update
    void Start()
    {
        GetChildren();
    }

    // Update is called once per frame
    void Update()
    {
        PlaceChildren();
    }

    /// <summary>
    /// Positions and scales children
    /// </summary>
    void PlaceChildren()
    {
        for (int i = 0; i < children.Count; i++)
        {
            children[i].pivot = new Vector2(0, 1);
            children[i].anchorMax = new Vector2(0, 0);  
            children[i].anchorMin = new Vector2(0, 0);
            float width = (rectTransform.rect.width + padding) / (float)children.Count;
            children[i].position = new Vector3(rectTransform.position.x + width * i * rectTransform.lossyScale.x, 
                rectTransform.position.y - rectTransform.rect.height * rectTransform.lossyScale.y, 
                children[i].position.z);
            children[i].sizeDelta = new Vector2(width - padding, children[i].sizeDelta.y);
        }
    }

    /// <summary>
    /// Gets a list of children for the current gameObject
    /// </summary>
    void GetChildren()
    {
        children = new List<RectTransform>();
        rectTransform = GetComponent<RectTransform>();
        foreach (RectTransform child in rectTransform)
        {
            children.Add(child);
        }
    }
}
