using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChunkLoader : MonoBehaviour
{
    public HeightMap hm;
    public Transform player;

    public void LoadChunks()
    {
        hm.InitBuffers();
        // iterate over x and y chunks
        int chunkIndex = 0;
        for (int y = 0; y < hm.mapSize.y; y++)
        {
            for (int x = 0; x < hm.mapSize.x; x++)
            {
                chunkIndex = hm.SetChunk(x, y);
            }
        }
        hm.ReleaseBuffers();

        // destroy any unused chunks
        int childCount = hm.children.Count;
        for (int i = chunkIndex + 1; i < childCount; i++)
        {
            if (hm.children.ContainsKey(i) && hm.children[i] != null)
                DestroyImmediate(hm.children[i].gameObject);
            hm.children.Remove(i);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
