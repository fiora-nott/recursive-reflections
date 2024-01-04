using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchBenchmarker : MonoBehaviour
{
    public CameraFree cam;

    public int chunkSize;

    private bool[] blocks;
    private int blockCount;

    private void OnDrawGizmos()
    {
        if (blocks != null)
        {
            Gizmos.color = Color.cyan;
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        if (GetBlock(new Vector3Int(x, y, z)))
                        {
                            Gizmos.DrawCube(new Vector3(x, y, z) + Vector3.one / 2, Vector3.one);
                        }
                    }
                }
            }
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.one * chunkSize / 2, Vector3.one * chunkSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(cam.GetPosition(), cam.GetForward() * 1000);
    }

    private bool GetBlock(Vector3Int pos)
    {
        return blocks[pos.z + chunkSize * (pos.y + pos.x * chunkSize)];
    }

    private bool InBlockBounds(Vector3Int pos)
    {
        return (pos.x >= 0 && pos.y >= 0 && pos.z >= 0 && pos.x < chunkSize && pos.y < chunkSize && pos.z < chunkSize);
    }

    private void Awake()
    {
        blockCount = chunkSize * chunkSize * chunkSize;
        blocks = new bool[blockCount];
        for (int i = 0; i < blockCount; i++)
        {
            blocks[i] = Random.Range(0, 100) < 10;
        }
    }

    public void BENCH_CRUDE_RAY()
    {
        float startTime = Time.realtimeSinceStartup;

        Vector3 rayPosition = cam.GetPosition();
        Vector3 rayDirection = cam.GetForward();
        Vector3Int blockPos;
        Vector3Int lastBlockPos = Vector3Int.zero;
        bool blockSolid;

        int steps = 0;
        while (steps < 5000)
        {
            steps++;

            blockPos = Vector3Int.FloorToInt(rayPosition);
            if (blockPos != lastBlockPos)
            {
                lastBlockPos = blockPos;
                if (InBlockBounds(blockPos))
                {
                    blockSolid = GetBlock(blockPos);
                    if (blockSolid == true)
                    {
                        break; //hit block
                    }
                }
            }
            rayPosition = rayPosition + rayDirection * 0.001f;
        }

        float functionTime = (Time.realtimeSinceStartup - startTime);
        Debug.Log("(millisecond) Time for Crude Ray: " + functionTime * 1000);
        Debug.Log("(microsecond) Time for Crude Ray: " + functionTime * 1000 * 1000);
    }

    public void BENCH_RAY_DDA()
    {
        float startTime = Time.realtimeSinceStartup;

        Vector3 rayOrigin = cam.GetPosition();
        Vector3 rayDirection = cam.GetForward();
        Vector3Int blockPos;
        bool blockSolid;

        int steps = 0;

        float functionTime = (Time.realtimeSinceStartup - startTime) * 1000;
        Debug.Log("(millisecond) Time for Ray DDA: " + functionTime * 1000);
        Debug.Log("(microsecond) Time for Ray DDA: " + functionTime * 1000 * 1000);
    }

    public void BENCH_PLANAR_DDA()
    {
        float startTime = Time.realtimeSinceStartup;



        float functionTime = (Time.realtimeSinceStartup - startTime) * 1000;
        Debug.Log("(millisecond) Time for Planar DDA: " + functionTime * 1000);
        Debug.Log("(microsecond) Time for Planar DDA: " + functionTime * 1000 * 1000);
    }

    public void BENCH_VOXEL_DDA()
    {
        float startTime = Time.realtimeSinceStartup;



        float functionTime = (Time.realtimeSinceStartup - startTime) * 1000;
        Debug.Log("(millisecond) Time for Voxel DDA: " + functionTime * 1000);
        Debug.Log("(microsecond) Time for Voxel DDA: " + functionTime * 1000 * 1000);
    }
}
