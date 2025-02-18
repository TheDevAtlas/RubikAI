using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FaceType { Up, Down, Front, Back, Left, Right }

public class Matrix : MonoBehaviour
{
    [Header("Cube Settings")]
    [Tooltip("The dimension of the cube (N x N, for N>=2).")]
    public int N = 3;
    [Tooltip("If true, spawn 0.1-scale cube GameObjects for visualization.")]
    public bool visualize = false;

    // Each face is stored as an N x N integer matrix.
    // (We use numbers 0–5 to represent colors; for example, 0 = white, 1 = yellow, 2 = blue, 3 = green, 4 = red, 5 = orange.)
    public int[,] Up;
    public int[,] Down;
    public int[,] Front;
    public int[,] Back;
    public int[,] Left;
    public int[,] Right;

    // A simple class to hold a spawned visual “sticker”
    public class VisualPiece
    {
        public FaceType face;
        public int row;
        public int col;
        public GameObject go;
    }
    List<VisualPiece> visualPieces = new List<VisualPiece>();

    // Colors corresponding to each face (order: Up, Down, Front, Back, Left, Right)
    Color[] faceColors = new Color[6] {
        Color.white,
        Color.yellow,
        Color.blue,
        Color.green,
        Color.red,
        new Color(1f, 0.5f, 0f) // orange
    };

    void Start()
    {
        InitializeCube();

        if (visualize)
        {
            SpawnVisualCubes();
            UpdateVisuals();
        }

        // Start scrambling so you can watch the moves happen.
        StartCoroutine(ScrambleCube());
    }

    // Initializes the six face matrices to a solved state.
    void InitializeCube()
    {
        Up = new int[N, N];
        Down = new int[N, N];
        Front = new int[N, N];
        Back = new int[N, N];
        Left = new int[N, N];
        Right = new int[N, N];

        // Fill each face with its “color” value.
        FillFace(Up, 0);
        FillFace(Down, 1);
        FillFace(Front, 2);
        FillFace(Back, 3);
        FillFace(Left, 4);
        FillFace(Right, 5);
    }

    void FillFace(int[,] face, int color)
    {
        for (int i = 0; i < N; i++)
            for (int j = 0; j < N; j++)
                face[i, j] = color;
    }

    #region Visualization

    // Spawns a small cube for every sticker on every face.
    void SpawnVisualCubes()
    {
        SpawnFaceVisual(FaceType.Up);
        SpawnFaceVisual(FaceType.Down);
        SpawnFaceVisual(FaceType.Front);
        SpawnFaceVisual(FaceType.Back);
        SpawnFaceVisual(FaceType.Left);
        SpawnFaceVisual(FaceType.Right);
    }

    void SpawnFaceVisual(FaceType face)
    {
        for (int i = 0; i < N; i++)
        {
            for (int j = 0; j < N; j++)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localScale = Vector3.one * 0.1f;
                cube.transform.position = GetVisualPosition(face, i, j);
                cube.transform.parent = this.transform; // for organization

                VisualPiece vp = new VisualPiece();
                vp.face = face;
                vp.row = i;
                vp.col = j;
                vp.go = cube;
                visualPieces.Add(vp);
            }
        }
    }

    // Compute a world position for a sticker given its face and grid coordinates.
    Vector3 GetVisualPosition(FaceType face, int row, int col)
    {
        float spacing = 0.12f;    // spacing between stickers
        Vector3 faceCenter = Vector3.zero;
        Vector3 rightDir = Vector3.right;
        Vector3 upDir = Vector3.up;
        float offset = 0.55f;     // distance from the cube center to the face center

        // Set the face’s center and the local directions.
        switch (face)
        {
            case FaceType.Up:
                faceCenter = Vector3.up * offset;
                rightDir = Vector3.right;
                upDir = Vector3.forward; // top of Up face will be in the +Z direction
                break;
            case FaceType.Down:
                faceCenter = Vector3.down * offset;
                rightDir = Vector3.right;
                upDir = Vector3.back;
                break;
            case FaceType.Front:
                faceCenter = Vector3.forward * offset;
                rightDir = Vector3.right;
                upDir = Vector3.up;
                break;
            case FaceType.Back:
                faceCenter = Vector3.back * offset;
                rightDir = Vector3.left;
                upDir = Vector3.up;
                break;
            case FaceType.Left:
                faceCenter = Vector3.left * offset;
                rightDir = Vector3.forward;
                upDir = Vector3.up;
                break;
            case FaceType.Right:
                faceCenter = Vector3.right * offset;
                rightDir = Vector3.back;
                upDir = Vector3.up;
                break;
        }

        float startOffset = (N - 1) / 2f;
        // Offset slightly along the face normal so cubes do not clip into the main cube.
        Vector3 pos = faceCenter
                      + ((col - startOffset) * spacing * rightDir)
                      + ((startOffset - row) * spacing * upDir)
                      + (GetFaceNormal(face) * 0.01f);
        return pos;
    }

    Vector3 GetFaceNormal(FaceType face)
    {
        switch (face)
        {
            case FaceType.Up: return Vector3.up;
            case FaceType.Down: return Vector3.down;
            case FaceType.Front: return Vector3.forward;
            case FaceType.Back: return Vector3.back;
            case FaceType.Left: return Vector3.left;
            case FaceType.Right: return Vector3.right;
        }
        return Vector3.zero;
    }

    // Update the color of each visual cube based on the state.
    void UpdateVisuals()
    {
        foreach (var vp in visualPieces)
        {
            int stateVal = GetFaceState(vp.face, vp.row, vp.col);
            Color col = faceColors[stateVal];
            Renderer rend = vp.go.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = col;
            }
        }
    }

    int GetFaceState(FaceType face, int row, int col)
    {
        switch (face)
        {
            case FaceType.Up: return Up[row, col];
            case FaceType.Down: return Down[row, col];
            case FaceType.Front: return Front[row, col];
            case FaceType.Back: return Back[row, col];
            case FaceType.Left: return Left[row, col];
            case FaceType.Right: return Right[row, col];
        }
        return -1;
    }

    #endregion

    #region Scrambling

    // Scramble the cube by applying a random move (with a short delay between moves)
    IEnumerator ScrambleCube()
    {
        yield return new WaitForSeconds(5f);
        // List of possible moves.
        string[] moves = new string[] { "U", "D", "F", "B", "L", "R", "M", "E", "S" };
        int scrambleMoves = 20;

        for (int i = 0; i < scrambleMoves; i++)
        {
            string move = moves[Random.Range(0, moves.Length)];
            bool clockwise = (Random.Range(0, 2) == 0);

            switch (move)
            {
                case "U": RotateU(clockwise); break;
                case "D": RotateD(clockwise); break;
                case "F": RotateF(clockwise); break;
                case "B": RotateB(clockwise); break;
                case "L": RotateL(clockwise); break;
                case "R": RotateR(clockwise); break;
                case "M": RotateM(clockwise); break;
                case "E": RotateE(clockwise); break;
                case "S": RotateS(clockwise); break;
            }
            UpdateVisuals();
            yield return new WaitForSeconds(1f);
        }
    }

    #endregion

    #region Rotation Helpers

    // Rotates a given face’s matrix 90° clockwise.
    // (This helper always does a clockwise rotation.)
    void RotateMatrix(int[,] matrix, bool clockwise)
    {
        // We assume clockwise here.
        int n = N;
        int[,] copy = new int[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                copy[i, j] = matrix[i, j];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                matrix[j, n - 1 - i] = copy[i, j];
    }

    // For each rotation below, if clockwise == false we simply call the clockwise version three times.

    public void RotateU(bool clockwise)
    {
        if (!clockwise) { for (int i = 0; i < 3; i++) RotateU(true); return; }

        RotateMatrix(Up, true);
        int[] temp = new int[N];
        for (int i = 0; i < N; i++) temp[i] = Front[0, i];
        for (int i = 0; i < N; i++) Front[0, i] = Left[0, i];
        for (int i = 0; i < N; i++) Left[0, i] = Back[0, i];
        for (int i = 0; i < N; i++) Back[0, i] = Right[0, i];
        for (int i = 0; i < N; i++) Right[0, i] = temp[i];
    }

    public void RotateD(bool clockwise)
    {
        if (!clockwise) { for (int i = 0; i < 3; i++) RotateD(true); return; }

        RotateMatrix(Down, true);
        int[] temp = new int[N];
        for (int i = 0; i < N; i++) temp[i] = Front[N - 1, i];
        for (int i = 0; i < N; i++) Front[N - 1, i] = Right[N - 1, i];
        for (int i = 0; i < N; i++) Right[N - 1, i] = Back[N - 1, i];
        for (int i = 0; i < N; i++) Back[N - 1, i] = Left[N - 1, i];
        for (int i = 0; i < N; i++) Left[N - 1, i] = temp[i];
    }

    public void RotateF(bool clockwise)
    {
        if (!clockwise) { for (int i = 0; i < 3; i++) RotateF(true); return; }

        RotateMatrix(Front, true);
        int[] temp = new int[N];
        // Save bottom row of Up.
        for (int i = 0; i < N; i++) temp[i] = Up[N - 1, i];
        // Up bottom row gets replaced by the rightmost column of Left (in reverse order).
        for (int i = 0; i < N; i++) Up[N - 1, i] = Left[N - 1 - i, N - 1];
        // Left’s right column gets the top row of Down.
        for (int i = 0; i < N; i++) Left[i, N - 1] = Down[0, i];
        // Down’s top row gets replaced by the leftmost column of Right (in reverse order).
        for (int i = 0; i < N; i++) Down[0, i] = Right[N - 1 - i, 0];
        // Right’s left column gets the saved Up bottom row.
        for (int i = 0; i < N; i++) Right[i, 0] = temp[i];
    }

    public void RotateB(bool clockwise)
    {
        if (!clockwise) { for (int i = 0; i < 3; i++) RotateB(true); return; }

        RotateMatrix(Back, true);
        int[] temp = new int[N];
        // Save top row of Up.
        for (int i = 0; i < N; i++) temp[i] = Up[0, i];
        // Up top row gets replaced by the rightmost column of Right (in reverse order).
        for (int i = 0; i < N; i++) Up[0, i] = Right[N - 1 - i, N - 1];
        // Right’s right column gets the bottom row of Down.
        for (int i = 0; i < N; i++) Right[i, N - 1] = Down[N - 1, i];
        // Down’s bottom row gets replaced by the leftmost column of Left (in reverse order).
        for (int i = 0; i < N; i++) Down[N - 1, i] = Left[N - 1 - i, 0];
        // Left’s left column gets the saved Up top row.
        for (int i = 0; i < N; i++) Left[i, 0] = temp[i];
    }

    public void RotateL(bool clockwise)
    {
        if (!clockwise) { for (int i = 0; i < 3; i++) RotateL(true); return; }

        RotateMatrix(Left, true);
        int[] temp = new int[N];
        // Save left column of Up.
        for (int i = 0; i < N; i++) temp[i] = Up[i, 0];
        // Up left column gets replaced by the right column of Back (reversed).
        for (int i = 0; i < N; i++) Up[i, 0] = Back[N - 1 - i, N - 1];
        // Back right column gets replaced by the left column of Down (reversed).
        for (int i = 0; i < N; i++) Back[i, N - 1] = Down[N - 1 - i, 0];
        // Down left column gets replaced by the left column of Front.
        for (int i = 0; i < N; i++) Down[i, 0] = Front[i, 0];
        // Front left column gets the saved Up left column.
        for (int i = 0; i < N; i++) Front[i, 0] = temp[i];
    }

    public void RotateR(bool clockwise)
    {
        if (!clockwise) { for (int i = 0; i < 3; i++) RotateR(true); return; }

        RotateMatrix(Right, true);
        int[] temp = new int[N];
        // Save right column of Up.
        for (int i = 0; i < N; i++) temp[i] = Up[i, N - 1];
        // Up right column gets replaced by the right column of Front.
        for (int i = 0; i < N; i++) Up[i, N - 1] = Front[i, N - 1];
        // Front right column gets replaced by the right column of Down.
        for (int i = 0; i < N; i++) Front[i, N - 1] = Down[i, N - 1];
        // Down right column gets replaced by the left column of Back (in reverse order).
        for (int i = 0; i < N; i++) Down[i, N - 1] = Back[N - 1 - i, 0];
        // Back left column gets the saved Up right column (in reverse order).
        for (int i = 0; i < N; i++) Back[i, 0] = temp[N - 1 - i];
    }

    // Middle rotations (for the “slice” between faces). For odd N the middle slice is used.
    public void RotateM(bool clockwise)
    {
        if (!clockwise) { for (int i = 0; i < 3; i++) RotateM(true); return; }

        int mid = N / 2;
        int[] temp = new int[N];
        // Save the middle column of Up.
        for (int i = 0; i < N; i++) temp[i] = Up[i, mid];
        for (int i = 0; i < N; i++) Up[i, mid] = Front[i, mid];
        for (int i = 0; i < N; i++) Front[i, mid] = Down[i, mid];
        for (int i = 0; i < N; i++) Down[i, mid] = Back[N - 1 - i, mid];
        for (int i = 0; i < N; i++) Back[i, mid] = temp[N - 1 - i];
    }

    public void RotateE(bool clockwise)
    {
        if (!clockwise) { for (int i = 0; i < 3; i++) RotateE(true); return; }

        int mid = N / 2;
        int[] temp = new int[N];
        // Save the middle row of Front.
        for (int i = 0; i < N; i++) temp[i] = Front[mid, i];
        for (int i = 0; i < N; i++) Front[mid, i] = Right[mid, i];
        for (int i = 0; i < N; i++) Right[mid, i] = Back[mid, i];
        for (int i = 0; i < N; i++) Back[mid, i] = Left[mid, i];
        for (int i = 0; i < N; i++) Left[mid, i] = temp[i];
    }

    public void RotateS(bool clockwise)
    {
        if (!clockwise) { for (int i = 0; i < 3; i++) RotateS(true); return; }

        int mid = N / 2;
        int[] temp = new int[N];
        // Save the middle row of Up.
        for (int i = 0; i < N; i++) temp[i] = Up[mid, i];
        for (int i = 0; i < N; i++) Up[mid, i] = Right[i, mid];
        for (int i = 0; i < N; i++) Right[i, mid] = Down[mid, N - 1 - i];
        for (int i = 0; i < N; i++) Down[mid, i] = Left[i, mid];
        for (int i = 0; i < N; i++) Left[i, mid] = temp[N - 1 - i];
    }

    #endregion
}
