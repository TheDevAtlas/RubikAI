using UnityEngine;

public class RubikGenerator : MonoBehaviour
{
    public int n_size = 3; // e.g., a 3x3x3 cube

    public GameObject cornerPiece;
    public GameObject facePiece;   // Face Piece Prefab
    public GameObject edgePiece;

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        // ─── Generate Corners ─────────────────────────────
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

        // ─── Generate Edges ─────────────────────────────
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
                    {
                        CreateEdge(x, y, z);
                    }
                }
            }
        }

        // ─── Generate Face Pieces ─────────────────────────────
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
                    {
                        CreateFace(x, y, z);
                    }
                }
            }
        }

        // Adjust the overall scale if needed
        transform.localScale = Vector3.one * (3f / n_size);
    }

    /// <summary>
    /// Creates a corner piece at grid coordinate (x, y, z).
    /// </summary>
    void CreateCorner(int x, int y, int z)
    {
        Vector3 pos = new Vector3(x, y, z)
                      - new Vector3(n_size - 1, n_size - 1, n_size - 1) / 2f
                      + transform.position;

        float yRotation = 0f;
        if (x == 0)
            yRotation = (z == 0) ? 0f : 90f;
        else // x is n_size - 1
            yRotation = (z == 0) ? 270f : 180f;

        Quaternion rotation = Quaternion.Euler(0f, yRotation, 0f);

        GameObject corner = Instantiate(cornerPiece, pos, rotation, transform);

        // For top corners, flip the Y scale.
        if (y == n_size - 1)
            corner.transform.localScale = new Vector3(100f, -100f, 100f);
    }

    /// <summary>
    /// Creates an edge piece at grid coordinate (x, y, z).
    /// </summary>
    void CreateEdge(int x, int y, int z)
    {
        Vector3 pos = new Vector3(x, y, z)
                      - new Vector3(n_size - 1, n_size - 1, n_size - 1) / 2f
                      + transform.position;

        Quaternion rotation = Quaternion.identity;

        // Determine rotation based on which two coordinates are extreme.
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

        // For top edge pieces, flip the Y scale.
        if (y == n_size - 1)
            edge.transform.localScale = new Vector3(100f, -100f, 100f);
    }

    /// <summary>
    /// Creates a face piece (with one visible side) at grid coordinate (x, y, z).
    /// The outward face is determined by the coordinate that is at an extreme.
    /// </summary>
    void CreateFace(int x, int y, int z)
    {
        Vector3 pos = new Vector3(x, y, z)
                      - new Vector3(n_size - 1, n_size - 1, n_size - 1) / 2f
                      + transform.position;

        Quaternion rotation = Quaternion.identity;

        // Because the face piece model is reversed (its painted side is on the back),
        // we reverse the direction used in LookRotation.
        if (x == 0)
        {
            // Left face: originally used -Vector3.right, so now use Vector3.right.
            rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        }
        else if (x == n_size - 1)
        {
            // Right face: originally used Vector3.right, so now use -Vector3.right.
            rotation = Quaternion.LookRotation(-Vector3.right, Vector3.up);
        }
        else if (y == 0)
        {
            // Bottom face: originally used -Vector3.up, so now use Vector3.up.
            rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
        }
        else if (y == n_size - 1)
        {
            // Top face: originally used Vector3.up, so now use -Vector3.up.
            rotation = Quaternion.LookRotation(-Vector3.up, Vector3.back);
        }
        else if (z == 0)
        {
            // Back face: originally used -Vector3.forward, so now use Vector3.forward.
            rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        }
        else if (z == n_size - 1)
        {
            // Front face: originally used Vector3.forward, so now use -Vector3.forward.
            rotation = Quaternion.LookRotation(-Vector3.forward, Vector3.up);
        }

        GameObject face = Instantiate(facePiece, pos, rotation, transform);

        // For top face pieces, flip the Y scale.
        if (y == n_size - 1)
            face.transform.localScale = new Vector3(100f, -100f, 100f);
    }
}
