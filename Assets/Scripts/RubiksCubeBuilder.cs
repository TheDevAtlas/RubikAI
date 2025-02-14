using UnityEngine;
using System.Collections.Generic;

public class RubiksCubeBuilder : MonoBehaviour
{
    public int cubeSize = 3; // N x N x N, N >= 2
    public GameObject centerPiece, edgePiece, cornerPiece;
    public bool animateMoves = true;
    public float moveDuration = 0.5f;

    private List<GameObject> cubePieces = new List<GameObject>();
    private Dictionary<Vector3Int, GameObject> piecePositions = new Dictionary<Vector3Int, GameObject>();

    void Start()
    {
        BuildCube();
        ScrambleCube();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ScrambleCube();
        }
    }

    void BuildCube()
    {
        for (int x = 0; x < cubeSize; x++)
        {
            for (int y = 0; y < cubeSize; y++)
            {
                for (int z = 0; z < cubeSize; z++)
                {
                    Vector3 position = new Vector3(x, y, z) - new Vector3(cubeSize - 1f, cubeSize - 1f, cubeSize - 1f) / 2f + transform.position;
                    GameObject piece;

                    if ((x == 0 || x == cubeSize - 1) && (y == 0 || y == cubeSize - 1) && (z == 0 || z == cubeSize - 1))
                    {
                        piece = Instantiate(cornerPiece, position, Quaternion.identity);
                        piece.transform.rotation = Quaternion.Euler(AdjustCornerRotation(x, y, z));
                    }
                    else if ((x == 0 || x == cubeSize - 1) && (y == 0 || y == cubeSize - 1) && (z == 0 || z == cubeSize - 1))
                    {
                        piece = Instantiate(edgePiece, position, Quaternion.identity);
                        piece.transform.rotation = Quaternion.Euler(AdjustEdgeRotation(x, y, z));
                    }
                    else
                    {
                        piece = Instantiate(centerPiece, position, Quaternion.identity);
                        piece.transform.rotation = Quaternion.Euler(AdjustCenterRotation(x, y, z));
                    }

                    piece.transform.parent = transform;
                    cubePieces.Add(piece);
                    piecePositions.Add(new Vector3Int(x,y,z), piece);
                }
            }
        }

        transform.localScale = new Vector3(3f / cubeSize, 3f / cubeSize, 3f / cubeSize);
    }

    Vector3 AdjustCornerRotation(int x, int y, int z)
    {
        return new Vector3(x * 90, y * 90, z * 90);
    }

    Vector3 AdjustEdgeRotation(int x, int y, int z)
    {
        return new Vector3((x == 0 ? 90 : 0), (y == 0 ? 90 : 0), (z == 0 ? 90 : 0));
    }

    Vector3 AdjustCenterRotation(int x, int y, int z)
    {
        return Vector3.zero; // Adjust as necessary
    }

    public void MakeMove(Vector3 axis, int layer)
    {
        // Perform rotation for a specific layer along an axis
    }

    void ScrambleCube()
    {
        // Implement scramble logic
    }
}
