using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class ChunkMemory : MonoBehaviour
{
    // The width, height, and depth of the chunk array will be this value
    [SerializeField, Range(1, 32)]
    public int chunkDomain;
    // The width of each given chunk. Determines local indexing arithmetic
    [SerializeField, Range(4, 1000)]
    public int chunkSize;

    // The sum of all chunks in the domain
    private int chunkCount;
    // Pointers to where each chunk's blocks are stored/indexed in the total block buffer
    private Chunk[] chunkPointers;

    // Draw an outline of the simulated world in the editor to better see camera position
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.one * chunkSize * chunkDomain / 2, Vector3.one * chunkSize * chunkDomain);
    }

    public ComputeBuffer CreateChunkBufferGPU()
    {
        // Safety call to ensure instantiated variables
        PopulateChunks();

        // Generate a pointer to GPU memory
        ComputeBuffer d_chunkPointers = new ComputeBuffer(chunkCount, sizeof(int));
        // Create an array of pointers to the chunk partitions of the block buffer
        int[] chunkPointers = new int[chunkCount];
        int blockCount = (int)Mathf.Pow(chunkSize, 3);
        for (int i = 0; i < chunkCount; i++)
        {
            int blockStartIndex = i;
            // The stride of each chunk is blockcount
            chunkPointers[i] = blockStartIndex * blockCount;
        }
        d_chunkPointers.SetData(chunkPointers);
        return d_chunkPointers;
    }
    
    public ComputeBuffer CreateBlockBufferGPU()
    {
        // Safety call to ensure instantiated variables
        PopulateChunks();

        int blocksPerChunk = chunkSize * chunkSize * chunkSize;
        ComputeBuffer d_blockData = new ComputeBuffer(chunkCount * blocksPerChunk, sizeof(int));
        // Will copy z-indexing from chunk ordering by default - flattened regardless
        for (int i = 0; i < chunkCount; i++)
        {
            Chunk chunk = chunkPointers[i];
            int blockDataIndex = i * blocksPerChunk;
            chunk.CopyBlocksIntoBuffer(d_blockData, blockDataIndex);
        }
        return d_blockData;
    }

    // Will create managed chunk memory and fill with random noise
    public void PopulateChunks()
    {
        if (chunkPointers == null)
        {
            chunkCount = chunkDomain * chunkDomain * chunkDomain;
            chunkPointers = new Chunk[chunkCount];
            int i = 0;
            for (int x = 0; x < chunkDomain; x++)
            {
                for (int y = 0; y < chunkDomain; y++)
                {
                    for (int z = 0; z < chunkDomain; z++)
                    {
                        chunkPointers[i] = new Chunk(chunkSize);
                        chunkPointers[i].Randomize();
                        i++;
                    }
                }
            }
        }
    }

    // Will create managed chunk memory with selected noise values
    public void PopulateChunksNoise(float noiseWidth, float noiseHeight)
    {
        if (chunkPointers == null)
        {
            chunkCount = chunkDomain * chunkDomain * chunkDomain;
            chunkPointers = new Chunk[chunkCount];
            int i = 0;
            for (int x = 0; x < chunkDomain; x++)
            {
                for (int y = 0; y < chunkDomain; y++)
                {
                    for (int z = 0; z < chunkDomain; z++)
                    {
                        chunkPointers[i] = new Chunk(chunkSize);
                        chunkPointers[i].FillNoise(x, y, z, chunkDomain, noiseWidth, noiseHeight);
                        i++;
                    }
                }
            }
        }
    }

    public int GetDomain()
    {
        return chunkDomain;
    }
    public int GetChunkSize()
    {
        return chunkSize;
    }
}
