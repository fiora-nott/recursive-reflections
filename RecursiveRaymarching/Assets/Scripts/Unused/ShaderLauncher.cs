/*
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderLauncher : MonoBehaviour
{
    //Voxel info
    public ChunkMemory chunkMemory;
    public ComputeBuffer d_chunkPointers;

    //Render Target info
    public Vector2Int screenResolution;
    [SerializeField]
    public ComputeShader computeShader;
    public RenderTexture renderTexture;

    //Camera info
    public Vector3 cameraPosition;
    //public Matrix4x4 cameraRotation;
    [SerializeField, Range(10, 1000)]
    public float cameraMoveSpeed;

    private void OnEnable()
    {
        d_chunkPointers = chunkMemory.CreatePointerBufferGPU();
    }

    private void OnDisable()
    {
        d_chunkPointers.Release();
        d_chunkPointers = null;
    }

    void Start()
    {
        CreateRenderTexture();
        DispatchShader();
    }

    private void CreateRenderTexture()
    {
        renderTexture = new RenderTexture(screenResolution.x, screenResolution.y, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
    }

    private void DispatchShader()
    {
        computeShader.SetTexture(0, "Result", renderTexture);
        computeShader.SetFloat("ScreenResolutionX", renderTexture.width);
        computeShader.SetFloat("ScreenResolutionY", renderTexture.height);

        computeShader.SetBuffer(0, "ChunkPointers", d_chunkPointers);
        computeShader.SetInt("ChunkDomain", chunkMemory.GetDomain());

        computeShader.SetVector("CameraPosition", cameraPosition);

        computeShader.Dispatch(0, renderTexture.width / 8, renderTexture.height / 8, 1);
    }

    private void Update()
    {
        float speedAdj = cameraMoveSpeed * Time.deltaTime;
        cameraPosition.x += Input.GetAxis("Vertical") * speedAdj;
        cameraPosition.z += Input.GetAxis("Horizontal") * speedAdj;
        if (Input.GetKey(KeyCode.Space))
        {
            cameraPosition.y += 1 * speedAdj;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            cameraPosition.y -= 1 * speedAdj;
        }
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (renderTexture == null)
        {
            CreateRenderTexture();
        }
        DispatchShader();
        Graphics.Blit(renderTexture, destination);
        //Debug.Log("Blit");
    }
}

*/