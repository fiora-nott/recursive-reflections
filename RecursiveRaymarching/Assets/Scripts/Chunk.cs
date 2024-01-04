using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class Chunk
{
    // Managed array of 32 bit block values.
    // Each block's value is an RGBA format where A represents solidness
    int[] blocks;
    private int size;
    private int blockCount;

    public Chunk(int _size)
    {
        size = _size;
        blockCount = size * size * size;
        blocks = new int[blockCount];

        // Each Int represents an RGBA color value parsed by the shader:
        // r8_g8_b8_a8
        // r = red
        // g = green
        // b = blue
        // a = alpha
    }

    private int IndexBlockData(int x, int y, int z)
    {
        return z + size * (y + x * size);
    }

    public void Randomize()
    {
        int i = 0;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    //This range represents the fractional chance of this being solid
                    bool isSolid = Random.Range(0, 32) == 0; 

                    // Randomize block colors within a given range to get semblances of order
                    blocks[IndexBlockData(x, y, z)] = PackInteger(
                        Random.Range(x * 15 % 255, x * 15 % 255),
                        Random.Range(y * 15 % 255, y * 15 % 255),
                        Random.Range(z * 15 % 255, z * 15 % 255),
                        isSolid ? 255 : 0);
                    
                }
            }
        }
    }

    public void FillNoise(int xo, int yo, int zo, int domain, float noiseWidth = 150.23f, float noiseHeight = 0.5f)
    {
        Debug.Log("filling Noise!");
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    // Convert from local block pos model space into world positions
                    float xReal = xo * size + x;
                    float yReal = yo * size + y;
                    float zReal = zo * size + z;
                    // Calculate perlin noise with our given horizontal positions
                    float noise = Mathf.PerlinNoise((float)xReal / noiseWidth, (float)zReal / noiseWidth);
                    int surfaceHeight = Mathf.FloorToInt(noise * size * domain * noiseHeight);
                    // If we are setting a block under the surface height calculated, make it solid
                    bool isSolid = yReal < surfaceHeight; //% of being solid

                    // Use RGB to represent gradients of the XYZ axis position
                    blocks[IndexBlockData(x, y, z)] = PackInteger(
                        (int)((float)xReal / (size * domain) * 255),
                        (int)((float)yReal / (size * domain) * 255),
                        (int)((float)zReal / (size * domain) * 255),
                        isSolid ? 255 : 0
                        );

                    /*
                    blocks[IndexBlockData(x,y,z)] = PackInteger(
                        255,
                        255,
                        255,
                        isSolid ? 255 : 0
                        );
                    */
                    /*
                    blocks[i] = PackInteger(
                        Random.Range(x * 15 % 255, x * 15 % 255),
                        Random.Range(y * 15 % 255, y * 15 % 255),
                        Random.Range(z * 15 % 255, z * 15 % 255),
                        (isSolid ? 255 : 0));
                    */
                }
            }
        }
    }

    public void CopyBlocksIntoBuffer(ComputeBuffer target, int startIndex)
    {
        target.SetData(blocks, 0, startIndex, blockCount);
    }

    // Used to reduce video memory usage by packing all color channels into one integer
    public int PackInteger(int r, int g, int b, int a)
    {
        // Parameters must be in the range 0 to 255 to avoid bit overflow
        if (r < 0 || r > 255 || g < 0 || g > 255 || b < 0 || b > 255 || a < 0 || a > 255)
        {
            Debug.LogError("Color value out of range");
            return 0;
        }

        return (
            (r << 24) |
            (g << 16) |
            (b << 8) |
            (a << 0)
            );
    }
}
