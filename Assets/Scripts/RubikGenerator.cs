using UnityEngine;

public class RubikGenerator : MonoBehaviour
{
    public int n_size;

    public GameObject centerPiece;
    public GameObject edgePiece;
    public GameObject cornerPiece;

    public Material[] faceMats;

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {

        // Build Rubik's Cube From Bottom To Top //
        for(int x = 0; x < n_size; x++)
        {
            for (int z = 0; z < n_size; z++)
            {
                for (int y = 0; y < n_size; y++)
                {
                    CheckCorner(x, y, z);
                }
            }
        }

        // Rescale Pieces For Screen Size //
        transform.localScale = Vector3.one * (3f / n_size);
    }

    public void CheckCorner(int x, int y, int z)
    {
        if(x == 0)
        {
            if(y == 0)
            {
                if(z == 0)
                {
                    GameObject n = Instantiate(cornerPiece, new Vector3(x,y,z) - new Vector3(n_size-1f, n_size-1f, n_size-1f)/ 2f + transform.position, Quaternion.identity);
                    n.transform.Rotate(new Vector3(0f, 0f, 0f));
                    n.transform.parent = transform;
                }
                
                if (z == n_size - 1)
                {
                    GameObject n = Instantiate(cornerPiece, new Vector3(x, y, z) - new Vector3(n_size - 1f, n_size - 1f, n_size - 1f) / 2f + transform.position, Quaternion.identity);
                    n.transform.Rotate(new Vector3(0f, 90f, 0f));
                    n.transform.parent = transform;
                }
            }

            if (y == n_size - 1)
            {
                if (z == 0)
                {
                    GameObject n = Instantiate(cornerPiece, new Vector3(x, y, z) - new Vector3(n_size - 1f, n_size - 1f, n_size - 1f) / 2f + transform.position, Quaternion.identity);
                    n.transform.Rotate(new Vector3(0f, 0f, 0f));
                    n.transform.localScale = new Vector3(100f, -100f, 100f);
                    n.transform.parent = transform;
                }

                if (z == n_size - 1)
                {
                    GameObject n = Instantiate(cornerPiece, new Vector3(x, y, z) - new Vector3(n_size - 1f, n_size - 1f, n_size - 1f) / 2f + transform.position, Quaternion.identity);
                    n.transform.Rotate(new Vector3(0f, 90f, 0f));
                    n.transform.localScale = new Vector3(100f, -100f, 100f);
                    n.transform.parent = transform;
                }
            }
        }

        if (x == n_size - 1)
        {
            if (y == 0)
            {
                if (z == 0)
                {
                    GameObject n = Instantiate(cornerPiece, new Vector3(x, y, z) - new Vector3(n_size - 1f, n_size - 1f, n_size - 1f) / 2f + transform.position, Quaternion.identity);
                    n.transform.Rotate(new Vector3(0f, 270f, 0f));
                    n.transform.parent = transform;
                }

                if (z == n_size - 1)
                {
                    GameObject n = Instantiate(cornerPiece, new Vector3(x, y, z) - new Vector3(n_size - 1f, n_size - 1f, n_size - 1f) / 2f + transform.position, Quaternion.identity);
                    n.transform.Rotate(new Vector3(0f, 180f, 0f));
                    n.transform.parent = transform;
                }
            }

            if (y == n_size - 1)
            {
                if (z == 0)
                {
                    GameObject n = Instantiate(cornerPiece, new Vector3(x, y, z) - new Vector3(n_size - 1f, n_size - 1f, n_size - 1f) / 2f + transform.position, Quaternion.identity);
                    n.transform.Rotate(new Vector3(0f, 270f, 0f));
                    n.transform.localScale = new Vector3(100f, -100f, 100f);
                    n.transform.parent = transform;
                }

                if (z == n_size - 1)
                {
                    GameObject n = Instantiate(cornerPiece, new Vector3(x, y, z) - new Vector3(n_size - 1f, n_size - 1f, n_size - 1f) / 2f + transform.position, Quaternion.identity);
                    n.transform.Rotate(new Vector3(0f, 180f, 0f));
                    n.transform.localScale = new Vector3(100f, -100f, 100f);
                    n.transform.parent = transform;
                }
            }
        }
    }
}
