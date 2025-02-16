using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class RubikGenerator : MonoBehaviour
{
    public int n_size = 3;
    public GameObject cornerPiece;
    public GameObject facePiece;
    public GameObject edgePiece;

    public Material[] capMaterials;

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        for (int ix = 0; ix < 2; ix++)
        {
            for (int iy = 0; iy < 2; iy++)
            {
                for (int iz = 0; iz < 2; iz++)
                {
                    int x = (ix == 0) ? 0 : n_size - 1;
                    int y = (iy == 0) ? 0 : n_size - 1;
                    int z = (iz == 0) ? 0 : n_size - 1;
                    CreateCorner(x, y, z);
                }
            }
        }

        for (int x = 0; x < n_size; x++)
        {
            for (int y = 0; y < n_size; y++)
            {
                for (int z = 0; z < n_size; z++)
                {
                    int extremeCount = 0;
                    if (x == 0 || x == n_size - 1) extremeCount++;
                    if (y == 0 || y == n_size - 1) extremeCount++;
                    if (z == 0 || z == n_size - 1) extremeCount++;
                    if (extremeCount == 2)
                        CreateEdge(x, y, z);
                }
            }
        }

        for (int x = 0; x < n_size; x++)
        {
            for (int y = 0; y < n_size; y++)
            {
                for (int z = 0; z < n_size; z++)
                {
                    int extremeCount = 0;
                    if (x == 0 || x == n_size - 1) extremeCount++;
                    if (y == 0 || y == n_size - 1) extremeCount++;
                    if (z == 0 || z == n_size - 1) extremeCount++;
                    if (extremeCount == 1)
                        CreateFace(x, y, z);
                }
            }
        }

        transform.localScale = Vector3.one * (3f / n_size);

        SetColors();
    }

    void CreateCorner(int x, int y, int z)
    {
        Vector3 pos = new Vector3(x, y, z) - new Vector3(n_size - 1, n_size - 1, n_size - 1) / 2f + transform.position;
        float yRotation = 0f;
        if (x == 0)
            yRotation = (z == 0) ? 0f : 90f;
        else
            yRotation = (z == 0) ? 270f : 180f;
        Quaternion rotation = Quaternion.Euler(0f, yRotation, 0f);
        GameObject corner = Instantiate(cornerPiece, pos, rotation, transform);
        if (y == n_size - 1)
            corner.transform.localScale = new Vector3(100f, -100f, 100f);
    }

    void CreateEdge(int x, int y, int z)
    {
        Vector3 pos = new Vector3(x, y, z) - new Vector3(n_size - 1, n_size - 1, n_size - 1) / 2f + transform.position;
        Quaternion rotation = Quaternion.identity;
        if ((x == 0 || x == n_size - 1) && (y == 0 || y == n_size - 1))
        {
            Vector3 forward = (x == 0) ? -Vector3.right : Vector3.right;
            Vector3 up = -Vector3.up;
            rotation = Quaternion.LookRotation(-forward, -up);
        }
        else if ((x == 0 || x == n_size - 1) && (z == 0 || z == n_size - 1))
        {
            Vector3 forward = (x == 0) ? -Vector3.right : Vector3.right;
            Vector3 up = (z == 0) ? -Vector3.forward : Vector3.forward;
            rotation = Quaternion.LookRotation(-forward, -up);
        }
        else if ((y == 0 || y == n_size - 1) && (z == 0 || z == n_size - 1))
        {
            Vector3 forward = (z == 0) ? -Vector3.forward : Vector3.forward;
            Vector3 up = -Vector3.up;
            rotation = Quaternion.LookRotation(-forward, -up);
        }
        GameObject edge = Instantiate(edgePiece, pos, rotation, transform);
        if (y == n_size - 1)
            edge.transform.localScale = new Vector3(100f, -100f, 100f);
    }

    void CreateFace(int x, int y, int z)
    {
        Vector3 pos = new Vector3(x, y, z) - new Vector3(n_size - 1, n_size - 1, n_size - 1) / 2f + transform.position;
        Quaternion rotation = Quaternion.identity;
        if (x == 0)
            rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        else if (x == n_size - 1)
            rotation = Quaternion.LookRotation(-Vector3.right, Vector3.up);
        else if (y == 0)
            rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
        else if (y == n_size - 1)
            rotation = Quaternion.LookRotation(-Vector3.up, Vector3.back);
        else if (z == 0)
            rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        else if (z == n_size - 1)
            rotation = Quaternion.LookRotation(-Vector3.forward, Vector3.up);
        GameObject face = Instantiate(facePiece, pos, rotation, transform);
        if (y == n_size - 1)
            face.transform.localScale = new Vector3(100f, -100f, 100f);
    }

    void SetColors()
    {
        List<GameObject> cubeFaces = new List<GameObject>();
        foreach(Transform t in transform)
        {
            foreach(Transform f in t)
            {
                // Top Face //
                if(Vector3.Dot(f.transform.forward, Vector3.down) >= 0.9f)
                {
                    f.GetComponent<Renderer>().material = capMaterials[0];
                }

                // Bottom Face //
                if (Vector3.Dot(f.transform.forward, Vector3.up) >= 0.9f)
                {
                    f.GetComponent<Renderer>().material = capMaterials[1];
                }

                // Forward Face //
                if (Vector3.Dot(f.transform.forward, Vector3.right) >= 0.9f)
                {
                    f.GetComponent<Renderer>().material = capMaterials[2];
                }

                // Right Face //
                if (Vector3.Dot(f.transform.forward, Vector3.forward) >= 0.9f)
                {
                    f.GetComponent<Renderer>().material = capMaterials[3];
                }

                // Backward Face //
                if (Vector3.Dot(f.transform.forward, Vector3.left) >= 0.9f)
                {
                    f.GetComponent<Renderer>().material = capMaterials[4];
                }

                // Left Face //
                if (Vector3.Dot(f.transform.forward, Vector3.back) >= 0.9f)
                {
                    f.GetComponent<Renderer>().material = capMaterials[5];
                }
            }
        }
    }
}
