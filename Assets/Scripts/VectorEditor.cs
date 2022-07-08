using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class VectorEditor : MonoBehaviour
{
    public float padding;
    RectTransform rectTransform;
    [HideInInspector]
    public List<RectTransform> children;
    Vector3 current;

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
