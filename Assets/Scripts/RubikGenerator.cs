using UnityEngine;

public class RubikGenerator : MonoBehaviour
{
    public int n_size = 3; // e.g., a 3x3x3 cube

    public GameObject cornerPiece;
    // public GameObject edgePiece;
    // public GameObject centerPiece;
    public Material[] faceMats;

    // ─── ADDED: Edge Piece Reference ────────────────────────────────
    public GameObject edgePiece;

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        // Instead of iterating through every cell,
        // only loop through the eight corner positions.
        for (int ix = 0; ix < 2; ix++)
        {
            for (int iy = 0; iy < 2; iy++)
            {
                for (int iz = 0; iz < 2; iz++)
                {
                    // Convert binary loop indices to actual positions (0 or n_size - 1)
                    int x = (ix == 0) ? 0 : n_size - 1;
                    int y = (iy == 0) ? 0 : n_size - 1;
                    int z = (iz == 0) ? 0 : n_size - 1;
                    CreateCorner(x, y, z);
                }
            }
        }

        // ─── ADDED: Generate Edge Pieces ───────────────────────────────
        // An edge piece has exactly two coordinates on the extremes (0 or n_size-1)
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

        // Adjust the overall scale if needed
        transform.localScale = Vector3.one * (3f / n_size);
    }

    /// <summary>
    /// Instantiates a corner piece at the given grid coordinates,
    /// applying the proper rotation and scale.
    /// </summary>
    void CreateCorner(int x, int y, int z)
    {
        // Calculate the position so that the cube is centered around the transform's position
        Vector3 pos = new Vector3(x, y, z)
                      - new Vector3(n_size - 1, n_size - 1, n_size - 1) / 2f
                      + transform.position;

        // Determine the Y rotation based on x and z.
        // Bottom pieces (y == 0) use one set of rotations; top pieces (y == n_size - 1) the same rotations but with a scale flip.
        float yRotation = 0f;
        if (x == 0)
            yRotation = (z == 0) ? 0f : 90f;
        else // x is n_size - 1
            yRotation = (z == 0) ? 270f : 180f;

        Quaternion rotation = Quaternion.Euler(0f, yRotation, 0f);

        // Instantiate the corner piece as a child of this transform.
        GameObject corner = Instantiate(cornerPiece, pos, rotation, transform);

        // For top corners, adjust the scale (as in your original code)
        if (y == n_size - 1)
            corner.transform.localScale = new Vector3(100f, -100f, 100f);
    }

    // ─── ADDED: CreateEdge Method ───────────────────────────────
    /// <summary>
    /// Instantiates an edge piece at the given grid coordinates,
    /// determining its proper rotation based on which two axes are at the extremes.
    /// </summary>
    void CreateEdge(int x, int y, int z)
    {
        // Calculate the position so that the cube is centered around the transform's position.
        Vector3 pos = new Vector3(x, y, z)
                      - new Vector3(n_size - 1, n_size - 1, n_size - 1) / 2f
                      + transform.position;

        // Determine rotation based on which two coordinates are at the extremes.
        Quaternion rotation = Quaternion.identity;

        // Edge on x-y plane (z is in between)
        if ((x == 0 || x == n_size - 1) && (y == 0 || y == n_size - 1))
        {
            Vector3 forward = (x == 0) ? -Vector3.right : Vector3.right;
            Vector3 up = -Vector3.up;
            rotation = Quaternion.LookRotation(-forward, -up);
        }
        // Edge on x-z plane (y is in between)
        else if ((x == 0 || x == n_size - 1) && (z == 0 || z == n_size - 1))
        {
            Vector3 forward = (x == 0) ? -Vector3.right : Vector3.right;
            Vector3 up = (z == 0) ? -Vector3.forward : Vector3.forward;
            rotation = Quaternion.LookRotation(-forward, -up);
        }
        // Edge on y-z plane (x is in between)
        else if ((y == 0 || y == n_size - 1) && (z == 0 || z == n_size - 1))
        {
            Vector3 forward = (z == 0) ? -Vector3.forward : Vector3.forward;
            Vector3 up = -Vector3.up;
            rotation = Quaternion.LookRotation(-forward, -up);
        }

        // Instantiate the edge piece as a child of this transform.
        GameObject edge = Instantiate(edgePiece, pos, rotation, transform);

        // For top edge pieces, adjust the scale (mirroring as done for corners)
        if (y == n_size - 1)
            edge.transform.localScale = new Vector3(100f, -100f, 100f);
    }
}
