using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
    public HeightMap hm;
    public Transform player;
    public Transform playerSpawn;
    Vector2 chunkPos;
    Vector2 center;

    public Vector2 PositionInChunkCoords(Vector2 pos)
    {
        Vector2 scl = Vector2.Scale(hm.scale, hm.transform.localScale) * hm.chunkSize;
        return new Vector2(pos.x / scl.x, pos.y / scl.y);
    }

    public void LoadChunks()
    {
        hm.InitBuffers();

        // get coords
        center = PositionInChunkCoords(new Vector2(player.position.x, player.position.z));
        center = new Vector2(Mathf.CeilToInt(center.x) - .5f, Mathf.CeilToInt(center.y) - .5f);
        Vector2 min = center - hm.mapSize / 2f;
        Vector2 max = center + hm.mapSize / 2f;

        // iterate over x and y chunks
        for (int y = (int)min.y; y < max.y; y++)
        {
            for (int x = (int)min.x; x < max.x; x++)
            {
                hm.SetChunk(x, y, false);

                if (y > center.y - 2f && x > center.x - 2f && y <= center.y + 1 && x <= center.x + 1)
                {
                    GameObject child = hm.GetChunk(x, y).gameObject;
                    if (child != null && child.GetComponent<MeshCollider>())
                        child.GetComponent<MeshCollider>().enabled = true;
                    else if (child != null && !child.GetComponent<MeshCollider>())
                        child.AddComponent<MeshCollider>();
                }
                else if (hm.GetChunk(x, y).GetComponent<MeshCollider>() != null)
                    hm.GetChunk(x, y).GetComponent<MeshCollider>().enabled = false;
            }
        }
        hm.ReleaseBuffers();

        // destroy any unused chunks
        List<Vector2> keys = hm.ChunkIndices();
        foreach (Vector2 key in keys)
        {
            if (key.x >= max.x || key.y >= max.y || key.x < min.x || key.y < min.y)
            {
                hm.RemoveChunk(key);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        player.position = playerSpawn.position;
        hm.ClearMesh();
        LoadChunks();
    }

    // Update is called once per frame
    void Update()
    {
        chunkPos = PositionInChunkCoords(new Vector2(player.position.x, player.position.z));
        Vector2 diff = new Vector2(Mathf.Abs(chunkPos.x - center.x), Mathf.Abs(chunkPos.y - center.y));
        if (diff.x >= 1f || diff.y >= 1f)
        {
            LoadChunks();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            player.position = playerSpawn.position;
            player.rotation = Quaternion.identity;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}
