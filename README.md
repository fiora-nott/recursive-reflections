## Overview
**Date :** June 2023  
**Languages :** HLSL, C#  
**Libraries :**  Unity Compute Shaders  
**IDE :**  Unity Game Engine  
**Purpose :**  Hobby/Learning  
**Learning Sources :**  Sebastian Lague's Raytracing, Voxelbee Devlogs, DDA Algorithm  
**Time Spent :** ~14 days  

## Summary
This project is a voxel raymarcher with recursive reflections. It is written in HLSL and presented using the Unity game engine. To start, there is a Unity camera in the scene. Its transformation is modified with mouse and keyboard input using a C# script. A second script overwrites the camera's blit operation. Each time the screen is drawn, the user settings are updated (reflection count, world color, etc). A third script carries the voxels in memory 32-bit RGBA values, subdivided as chunks and copied into compute buffers. For demonstration purposes, there is a perlin noise function and a random-value setting when populating the voxel memory. All of the information in these scripts is passed into the shader as uniforms or buffers. The shader runs for each pixel of the screen, instantiating rays at the camera position. Perspective is applied to each ray based on the pixel's screen position. This simulates a camera frustrum. All rays are now calculated concurrently on the GPU. Each ray completes the following operations: 

1) Apply perspective using the screenspace UV coordinates associated with our pixel.
2) Geometrically calculate where this ray intersects with the global volume of chunks.
3) Step forward to the intersection with world bounds or return a mask if no intersection occurs.
4) Pass this valid ray position and direction into the Raymarch function.
5) The Raymarch function procedurally steps the ray position forward using the Digital Differential Analyzer algorithm (DDA).
6) Once a solid block (determined by the alpha channel) is hit, return the block's RGB value with a dot-product shadow applied.
7) The raymarch result is added to a running color sum. If reflections are off, the algorithm ends here with an opaque output.
8) If reflections are on and a hit was successful, the raymarch function is recursively called and its results are added to the color sum.
9) These recursive calls use the previous hit's data to instantiate a new ray at a reflected angle determined by the hit's normal.
10) The final color sum is applied to the corresponding screen-buffer pixel and returned for drawing.

The result is blazingly fast because the DDA algorithm is extremely efficient at traversing dense voxel space. DDA steps in block-wise space by hitting the next-closest axis, determined by the ray's direction. DDA only requires comparing the ratio between similar triangles. Once a block is hit, locating it in memory only takes O(n) time. This is because the block buffer is flattened into a 1d array. The buffer is organized by chunk strides in XYZ space, then their given blocks in XYZ space. This is similar to a z-order curve.

The final result is an algorithm that can render a complicated volume of 134,217,728 voxels with up to 10 reflections at a speed of 60-90 fps. 

## Takeaways
- Function parameters can become extremely complicated in HLSL if you do not abstract your variables
- DDA for voxels can introduce severe artifacts. Stepping in block-space prevents gaps forming at edges/corners
- Raymarching requires enormous video memory if you want O(n) block accesses
- Debugging visual glitches in a mature shader requires an intense and thorough process, often with creative color coding
- Instead of using HLSL to draft, plan out all of the expected behaviour on paper before writing code
- Reflections past 2-3 begin losing coherence and introducing noise. Refraction or blurring may solve this

## Video Montage

Footage of the completed application running various scenes:

▶️ [Demonstration <3:31>](https://youtu.be/UM7M4-LG8Kc)

<!--
---

Compilation of the creation process with bloopers:

▶️ [Creation <>]([]))
-->

## Screenshots



![screenshot 0](Screenshots/mountain.gif)

---


![screenshot 0](Screenshots/screenshot0.png)

---


![screenshot 0](Screenshots/screenshot0.png)

---


![screenshot 0](Screenshots/screenshot0.png)

---


![screenshot 0](Screenshots/screenshot0.png)

---


![screenshot 0](Screenshots/screenshot0.png)

## Code Highlights

Vertex Shader (PerspectiveBinaryMem.shader):

```hlsl
void main() {
    // Determine 3d coordinate of this vertex with first 15 bits
    // 5 bits per axis allows 2^5 = 32 possible values. Each chunk is size 31x31x31 to fit within this range.
    // This downsizing is necessary since edge vertices will project their vertex coordinates 1 above the max bound (32 here)
    float x = float((binVertex & 0x1Fu) >> 0u);
    float y = float((binVertex & 0x3E0u) >> 5u);
    float z = float((binVertex & 0x7C00u) >> 10u);

    // Convert the vertex coordinate from model/chunk/local space to world space
    x += u_ChunkPos.x;
    y += u_ChunkPos.y;
    z += u_ChunkPos.z;

    // Collate a coordinate value and multiply by Model-View-Project matrix to generate screen position
    // This is passed to the fragment shader for scanline rendering
    vec4 worldPos = vec4(x, y, z, 1.0);
    gl_Position = u_MVP * worldPos;

    // Parse additional bit flags from the following layout:
    // 3-10-2-5-5-5
    // LightValue-TextureID-BlockID-z-y-x
    uint uvID = (binVertex >> 15u) & 3;
    uint textureID = (binVertex >> 17u) & 1023;
    uint lightValue = ((binVertex >> 27u) & 7);
    float lightValueF = float(lightValue) / 7;

    // Pass final calculations into the fragment shader
    v_TexCoord = texCoords[uvID];
    v_lightValue = lightValueF;
}
```

Meshing Algorithm (ChunkMesher.cpp):
```hlsl
// Iterate over each face of this voxel
for (int side = 0; side < 6; side++) {
    // Project out with a normal vector and check if the neighbor voxel is solid or air
    checkPos = blockPos + sideNormals[side];

    // If the neighbor is opaque/non-air, do not draw a face. This face is occluded.
    if (GetBlockFreeform(mem, checkPos, chunk) != Block::AIR)
        continue;
    
    // All conditions are met so draw the side
    float* localVerts = sideVertices[side];
    
    // i represents vertex number
    for (int i = 0; i < 4; i++) {
        // Grab the vertex displacement based on which vertex we are assigning
        int3 vertLocalPos = int3(localVerts[i * 3], localVerts[i * 3 + 1], localVerts[i * 3 + 2]);
        int3 vertObjectPos = blockPos + vertLocalPos;
    
        // Ambient occlusion actually has a bias since quads are triangles, and will work differently
        // at different diagonals for every plane.
        unsigned int aoLevel = 0; //must be 0-2 since light is 2-7
        int3 aoOffset = ((vertLocalPos * 2) - 1);
        int3 aoCheckPos1 = checkPos + aoDirections[side][i][0];
        int3 aoCheckPos2 = checkPos + aoDirections[side][i][1];
        int3 aoCheckPos3 = checkPos + aoDirections[side][i][2];
    
        // Check aoCheckPos1 last since its the corner block, which has the weakest shadow
        // Having an effect of 1 for every case creates shadows of equal intensity for edges and corners
        if      (GetBlockFreeform(mem, aoCheckPos3, chunk) != Block::AIR)
            aoLevel = 1;
        else if (GetBlockFreeform(mem, aoCheckPos2, chunk) != Block::AIR)
            aoLevel = 1;
        else if (GetBlockFreeform(mem, aoCheckPos1, chunk) != Block::AIR)
            aoLevel = 1; //could be 1 for lighter corners

        // Determine block values that will need to be packed into this vertex
        int uvID = i;
        int blockID = (int)(blockType); // must be less than 1024
        int lightValue = sideLightValues[side] - aoLevel; //must be between 0-7. AO is 0-2

        // Bitpack all the information calculated so far. This will be parsed by the bitwise shader.
        // Packing this information reduces memory footprint from 32 * 7 = 224 bits down to 32 bits.
        // That is a video memory reduction of SEVEN times!
        //each position is 0-31   (32)   so we use 15 bits, 5 for each pos
        //the uv        is 0-3    (4)    so we use 2 bits
        //the blockID   is 0-1023 (1024) so we use 10 bits
        //the lightVal  is 0-7    (8)    so we use 3 bits
        //the total bitcount is 30	 bits
        GLuint compressedVertex = vertObjectPos.x | vertObjectPos.y << 5 | vertObjectPos.z << 10 | uvID << 15 | blockID << 17 | lightValue << 27;
        mesh.PushVertex(compressedVertex);
        
        // Debugging tool: print out binary data of any vertices of block 0, 0, 0
        //if (blockPos.x == 0 && blockPos.y == 0 && blockPos.z == 0) {
        //	std::cout << std::bitset<32>(compressedVertex) << std::endl;
    }

    // Push indices in an order that designates 2 clockwise triangles, composing a quad with 4 vertices
    unsigned int indexStart = (mesh.index_count / 6) * 4;
    mesh.PushIndices({
        indexStart,
        indexStart + 3,
        indexStart + 1,
        indexStart,
        indexStart + 2,
        indexStart + 3 });
}
```
