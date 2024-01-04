using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class OctreeMemory : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        if (tree != null)
        {
            Gizmos.color = Color.green;
            DrawRegion(0, Vector3Int.zero, (int)Mathf.Pow(2, scale));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            Vector3Int blockPos = new Vector3Int(
                (int)Random.Range(0, Mathf.Pow(2, scale)),
                (int)Random.Range(0, Mathf.Pow(2, scale)),
                (int)Random.Range(0, Mathf.Pow(2, scale))
                );
            AddBlock(blockPos, 1, 0, Vector3Int.zero);
            //Debug.Log("ADDING BLOCK");
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log(tree.Count);
            for (int i = 0; i < tree.Count; ++i)
            {
                Debug.Log(i + ": " + tree[i]);
            }
        }
    }

    public void DrawRegion(int nodeIndex, Vector3 origin, int size)
    {
        if (size < 1) return;
        if (tree == null) return;

        //Debug.Log(nodeIndex);
        uint nodeData = tree[nodeIndex];
        bool isSubdivided = IsSubdivided(nodeData);
        //Debug.Log(nodeIndex + "..." + isSubdivided + "___" + ParseBlockData(nodeData));
        if (isSubdivided)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(origin + Vector3.one * size / 2, Vector3.one * size);
            int childIndex = (int)ParseChildrenPointer(nodeData);
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 newOrigin = origin + new Vector3(x, y, z)*size/2;
                        int offset = z + 2 * (y + x * 2);
                        int childIndexAdj = childIndex + offset;
                        DrawRegion(childIndexAdj, newOrigin, size / 2);
                    }
                }
            }
        } else
        {
            uint blockID = ParseBlockData(nodeData);
            if (blockID == 0)
            {

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(origin + Vector3.one * size / 2, Vector3.one * size * 0.99f);
                //Gizmos.color = Color.gray;
                //Gizmos.DrawCube(origin + Vector3.one * size / 2, Vector3.one * size);
            } else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(origin + Vector3.one * size / 2, Vector3.one * size);
            }
        }
    }


    // Determines the size of the octree in 2^size
    [SerializeField, Range(0, 16)]
    int scale;

    List<uint> tree;
    // Stores the next free position in the array
    // value of 2 = index 16 to 23 in the list
    int nextFreePosition;

    private void Awake()
    {
        Create();

        Vector3Int blockPos = new Vector3Int(0, 0, 0);
        uint blockID = 1;
        int root = 0;

        //AddBlock(blockPos, blockID, root, Vector3Int.zero);

        //Subdivide(0);
    }

    public void Create()
    {
        // add a leaf node of air as the root
        tree = new List<uint>();
        uint root = 0;
        tree.Add(root);
        // ensure our root node doesnt mess up 8-int indexing
        for (int i = 0; i < 7; i++)
        {
            tree.Add(0);
        }
        // remember which position to subdivide into
        nextFreePosition = 1;
    }

    public uint Subdivide(int nodeIndex)
    {
        // find node in question
        uint node = tree[nodeIndex];

        // save its current value as leaf for propagation
        uint childValue = node;

        // get a pointer to where the children will be (downscaled)
        int childrenIndex = nextFreePosition;
        nextFreePosition++;

        // construct a pointer node using the child index
        uint parentNode = ((uint)childrenIndex | (0x80000000));

        // upscale the pointer to the real index
        childrenIndex *= 8;

        // Assign new parent value
        tree[nodeIndex] = parentNode;

        // Write all children values
        tree.Capacity = tree.Capacity + 8;
        for (int i = 0; i < 8; i++)
        {
            tree.Add(0);
            tree[childrenIndex + i] = childValue;
        }

        return parentNode;

        //Debug.Log(parentNode);
        //Debug.Log(tree[childrenIndexAdj + 0]);
        //Debug.Log(childrenIndex);
        //Debug.Log(childrenIndexAdj);
        //Debug.Log(childValue);
        //Debug.Log(nextFreePosition);
        //Debug.Log(tree.Count);
    }

    public void AddBlock(Vector3Int pos, uint newBlockID, uint nodeData, Vector3Int parentPos, int size = -1)
    {
        /*
        if (size == -1)
            size = (int)Mathf.Pow(2, scale);

        while (IsSubdivided(nodeData))
        {
            Vector3Int planes = parentPos + Vector3Int.one * size / 2;
            Vector3Int childPos;
            Vector3Int childOffset;
            int childIndex = (int)FindChild(pos, nodeData, planes, out childOffset);
            childPos = parentPos + childOffset * size / 2;

            nodeData = tree[childIndex];
        }
        // If subdivided, recurse down to the correct leaf
        */
    }

    public void AddBlock(Vector3Int pos, uint blockValue, int nodeIndex, Vector3Int parentPos, int size = -1)
    {
        if (size == -1)
            size = (int)Mathf.Pow(2, scale);

        //Debug.Log(nodeIndex);

        uint nodeData = tree[nodeIndex];
        // If subdivided, recurse down to the correct leaf
        if (IsSubdivided(nodeData))
        {
            Vector3Int planes = parentPos + Vector3Int.one * size / 2;
            Vector3Int childPos;
            Vector3Int childOffset;
            int childIndex = (int)FindChild(pos, nodeData, planes, out childOffset);
            childPos = parentPos + childOffset * size / 2;

            AddBlock(pos, blockValue, childIndex, childPos, size / 2);
        } 
        // If looking at a leaf, see if we can end
        else {
            uint blockID = ParseBlockData(nodeData);
            // If the leaf is already the same block we are setting, do nothing
            if (blockID == blockValue)
            {
                return;
            } 
            // The leaf does not match our new block
            else {
                // If the leaf is at the smallest size, just overwrite its value
                if (size == 1)
                {
                    tree[nodeIndex] = 0 | blockValue;
                }
                // If the leaf can be subdivided into higher detail, do that
                else
                {
                    nodeData = Subdivide(nodeIndex);

                    Vector3Int planes = parentPos + Vector3Int.one * size / 2;
                    Vector3Int childPos;
                    Vector3Int childOffset;
                    int childIndex = (int)FindChild(pos, nodeData, planes, out childOffset);
                    childPos = parentPos + childOffset * size/2;

                    AddBlock(pos, blockValue, childIndex, childPos, size /2);
                }
            }
        }
    }

    public bool IsSubdivided(uint nodeData)
    {
        return (nodeData >> 31) == 1 ? true : false;
    }

    public uint ParseBlockData(uint nodeData)
    {
        return nodeData & 0x7FFFFFFF;
    }
    public uint ParseChildrenPointer(uint nodeData)
    {
        return (nodeData & 0x7FFFFFFF) * 8;
    }

    public uint FindChild(Vector3Int pos, uint nodeData, Vector3Int planes, out Vector3Int childOffset)
    {
        uint childIndex = ParseChildrenPointer(nodeData);
        childOffset = Vector3Int.zero;

        //Debug.Log("base index: " +  childIndex);
        if (pos.x >= planes.x)
        {
            childIndex += 4;
            childOffset.x = 1;
            //Debug.Log("x+" + childIndex);
        }
        if (pos.y >= planes.y)
        {
            childIndex += 2;
            childOffset.y = 1;
            //Debug.Log("y+" + childIndex);
        }
        if (pos.z >= planes.z)
        {
            childIndex += 1;
            childOffset.z = 1;
            //Debug.Log("z+" + childIndex);
        }
        return childIndex;
    }
}
