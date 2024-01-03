using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class OctreeCPU : MonoBehaviour
{
    public List<uint> tree;
    public Vector3Int origin;
    public int scale;

    private int diameter;

    private void Awake()
    {
        diameter = (int)Mathf.Pow(2, scale);
        tree = new List<uint>();
    }

    public void SetBlock(Vector3Int blockPos, uint blockID)
    {
        Vector3Int regionOrigin = origin;
        int size = diameter;
        int nodePtr = 0;
        uint nodeData = GetNodeData(nodePtr);

        // Keep going down the divisions until we hit a leaf
        while (
            GetDivided(nodeData) == true ||
            (GetBlockID(nodeData) != blockID && CanDivide(size))
            )
        {
            if (!GetDivided(nodeData))
                SubDivide(nodePtr);
            nodePtr = GetChildrenIndex(nodeData);
            nodePtr += GetChildOffset(regionOrigin, blockPos, size);
            nodeData = GetNodeData(nodePtr);
            // NEED TO UPDATE ORIGIN + REGION
        }
        // We have hit a leaf
        if (GetBlockID(nodeData) == blockID)
            return;
        // We need to overwrite
        SetLeaf(nodePtr, blockID);
    }


    public bool CanDivide(int size)
    {
        return size <= 1 ? false : true;
    }
        
    public void SubDivide(int nodePtr)
    {
        uint blockID = GetNodeData(nodePtr);
        int childPtr = tree.Count;
        for (int i = 0; i < 8; i++) {
            AddLeaf(blockID);
        }
        SetBranch(nodePtr, childPtr);
    }

    public void InsertBlock()
    {
    }

    // ACCESSORS
    public uint GetNodeData(int nodePtr)
    {
        return tree[nodePtr];
    }
    public bool GetDivided(uint nodeData)
    {
        return (nodeData >> 31) == 1 ? true : false;
    }
    public int GetBlockID(uint nodeData)
    {
        return (int)(nodeData & 0x7FFFFFFF);
    }
    public int GetChildrenIndex(uint nodeData)
    {
        return (int)(nodeData & 0x7FFFFFFF);
    }
    public int GetChildOffset(Vector3Int orig, Vector3Int blockPos, int size)
    {
        int off = 0;
        if (blockPos.x >= orig.x + size / 2)
            off += 4;
        if (blockPos.y >= orig.y + size / 2)
            off += 2;
        if (blockPos.z >= orig.z + size / 2)
            off += 1;
        return off;
    }

    // MUTATORS
    public void SetBranch(int nodePtr, int childPtr)
    {
        tree[nodePtr] = 0x80000000 | (uint)childPtr;
    }
    public void AddLeaf(uint blockID)
    {
        tree.Add(blockID);
    }
    public void SetLeaf(int nodePtr, uint blockID)
    {
        tree[nodePtr] = blockID;
    }

    /*
    // MUTATORS
    public void SetDivided(int nodePtr, bool isDivided)
    {
        if (isDivided)
        {
            tree[nodePtr] = tree[nodePtr] | 0x80000000;
        } else
        {
            tree[nodePtr] = tree[nodePtr] & 0x7FFFFFFF;
        }
    }
    public void SetBlockID(int nodePtr, uint blockID)
    {
        tree[nodePtr] = blockID;
    }
    public void SetChildIndex(int nodePtr, uint childIndex)
    {
        tree[nodePtr] = blockID;
    }
    */
}
