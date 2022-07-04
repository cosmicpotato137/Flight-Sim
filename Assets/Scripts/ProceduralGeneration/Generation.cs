using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generation : MonoBehaviour
{
    public BoxCollider bounds;
    public int numBoxes = 10;
    public int seed = 12345;
    public Vector2 noiseMin = new Vector2(0, 0);
    public Vector2 noiseMax = new Vector2(1, 1);

    //public Vector3 positionMultiplier;
    public Vector3 scaleMultiplier;
    public GameObject box; 

    private List<GameObject> boxes = new List<GameObject>();
    // Start is called before the first frame update

    private void OnValidate()
    {
        foreach (Transform child in transform)
            boxes.Add(child.gameObject);
        if (boxes.Count == 0)
            SetLayout();
    }

    void Start()
    {
        if (boxes.Count == 0)
            SetLayout();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ClearBoxes()
    {
        if (boxes.Count > 0)
        {
            foreach (GameObject box in boxes)
            {
                DestroyImmediate(box);
            }
        }
    }

    public void ResetLayout()
    {
        ClearBoxes();
        SetLayout();
    }

    public void SetLayout()
    {
        Random.InitState(seed);
        for (int i = 0; i < numBoxes; i++)
        {
            float ox = Mathf.PerlinNoise(Random.Range(noiseMin.x, noiseMax.x), Random.Range(noiseMin.y, noiseMax.y));
            float oy = Mathf.PerlinNoise(Random.Range(noiseMin.x, noiseMax.x), Random.Range(noiseMin.y, noiseMax.y));
            float oz = Mathf.PerlinNoise(Random.Range(noiseMin.x, noiseMax.x), Random.Range(noiseMin.y, noiseMax.y));

            //Vector3 pos = Vector3.Scale(new Vector3(x, y, z), bounds.size);
            Vector3 pos = bounds.center - bounds.size / 2;
            pos += Vector3.Scale(bounds.size, new Vector3(ox, oy, oz));
            //Quaternion rot = Quaternion.Euler(x, y, z);

            box.transform.localScale = scaleMultiplier;
            boxes.Add(Instantiate(box, pos, Quaternion.identity, transform));
            box.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private Vector3 RandomVector3(Vector3 max, Vector3 min)
    {
        float x = Random.Range(max.x, min.x);
        float y = Random.Range(max.y, min.y);
        float z = Random.Range(max.z, min.z);
        return new Vector3(x, y, z);
    }
    
    private Vector3 RandomVector2(Vector2 max, Vector2 min)
    {
        float x = Random.Range(max.x, min.x);
        float y = Random.Range(max.y, min.y);
        return new Vector2(x, y);
    }
}
