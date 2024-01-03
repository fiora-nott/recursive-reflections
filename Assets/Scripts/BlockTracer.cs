using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlockTracer : MonoBehaviour
{
    // Control variables accessible in the inspector
    public bool UseShadows = false;
    [Range(0, 50)]
    public int MaximumReflections = 1;
    public bool UseColorOverwrite = false;
    public Color OverwriteValue = Color.white;

    // Noise Variables to set before runtime
    [Range(0.1f, 1.0f)]
    public float NoiseHeight = 0.5f;
    [Range(20.0f, 300.0f)]
    public float NoiseWidth = 100.0f;
    public bool RandomWorld = false;

    // References to files
    public ComputeShader shader;
    public ChunkMemory chunkMemory;
    // Reference to camera moving script
    public CameraFree cam;
    // Customizable resolution for the simulated frustrum
    public Vector2Int screenResolution;

    // Buffer declarations
    public ComputeBuffer blockDataBuffer;
    public ComputeBuffer chunkPointerBuffer;

    // Generated in code and uploaded to the screen
    public RenderTexture renderTexture;

    private void DispatchShader()
    {
        if (blockDataBuffer == null)
        {
            if (RandomWorld)
            {
                // Fill random blocks with color like a starscape
                chunkMemory.PopulateChunks();
            } else
            {
                // Use the user-input perlin noise values to create hills
                chunkMemory.PopulateChunksNoise(NoiseWidth, NoiseHeight);
            }
            blockDataBuffer = chunkMemory.CreateBlockBufferGPU();
            chunkPointerBuffer = chunkMemory.CreateChunkBufferGPU();
        }
        // Hold a back-buffer equivalent in the form of a render image
        if (renderTexture == null)
        {
            // Screenresolution will be used to determine ray # and calculation time
            renderTexture = new RenderTexture(screenResolution.x, screenResolution.y, 24);
            renderTexture.enableRandomWrite = true;
            renderTexture.filterMode = FilterMode.Point;
            renderTexture.Create();
        }
        // Result represents the pixel buffer
        shader.SetTexture(0, "Result", renderTexture);
        // Apply the camera properties from the Unity transform
        shader.SetFloat("ScreenResolutionX", renderTexture.width);
        shader.SetFloat("ScreenResolutionY", renderTexture.height);
        shader.SetVector("CameraPosition", cam.GetPosition());
        shader.SetVector("CameraForward", cam.GetForward());
        shader.SetVector("CameraRight", cam.GetRight());
        shader.SetVector("CameraUp", cam.GetUp());
        // Upload links to the buffers
        shader.SetBuffer(0, "BlockBuffer", blockDataBuffer);
        shader.SetBuffer(0, "ChunkBuffer", chunkPointerBuffer);
        // Used for z-order indexing
        shader.SetInt("ChunkDomain", chunkMemory.GetDomain());
        shader.SetInt("ChunkSize", chunkMemory.GetChunkSize());
        // User inputs
        shader.SetInt("MaxReflects", MaximumReflections);
        shader.SetInt("ShadowsOn", UseShadows ? 1 : 0);
        shader.SetInt("OverwriteColor", UseColorOverwrite ? 1 : 0);
        shader.SetVector("OverwriteValue", OverwriteValue);

        // Run the shader with a pool size based on the resolution
        shader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
    }

    // Called each frame in place of an update function
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        DispatchShader();
        // Send the render texture to the screen
        Graphics.Blit(renderTexture, destination);
    }

    // Free all memory when the game ends or when the scene object is disabled
    private void OnDisable()
    {
        renderTexture = null;
        if (blockDataBuffer != null)
        {
            blockDataBuffer.Release();
            blockDataBuffer = null;
        }
        if (chunkPointerBuffer != null)
        {
            chunkPointerBuffer.Release();
            chunkPointerBuffer = null;
        }
    }
}
