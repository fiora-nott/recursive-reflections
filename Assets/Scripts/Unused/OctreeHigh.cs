using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3Int position;
    public int size;
    public int blockID = 0;

    public bool subdivided = false;
    public Node[] children;
    
    public Node(Vector3Int position, int size, int blockID)
    {
        this.position = position;
        this.size = size;
        this.blockID = blockID;
    }

    public void Subdivide()
    {
        if (size == 1)
            return;
        subdivided = true;

        children = new Node[8];
        int i = 0;
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    children[i++] = new Node(
                        position + new Vector3Int(x,y,z) * size / 2,
                        size / 2,
                        blockID
                        );
                }
            }
        }
    }
}

public class OctreeHigh : MonoBehaviour
{
    public int scale;
    public Node root;

    private void Awake()
    {
        root = new Node(Vector3Int.zero, (int)(Mathf.Pow(2, scale)), 0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Node n = root;
            while (n.subdivided == true)
            {
                n = n.children[0];
            }
            n.Subdivide();
        }
    }

    private void OnDrawGizmos()
    {
        if (root != null)
        {
            DrawNode(root);
        }
    }
    public void DrawNode(Node n)
    {
        if (n.subdivided)
        {
            //Vector3 size = Vector3.one * n.size * 1.001f;
            //Vector3 pos = n.position + size / 2;
            //Gizmos.color = Color.white;
            //Gizmos.DrawWireCube(pos, size);
            for (int i = 0; i < 8; i++)
            {
                DrawNode(n.children[i]);
            }
        } else
        {
            Vector3 size = Vector3.one * n.size;
            Vector3 pos = n.position + size / 2;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(pos, size);
        }
    }
}
