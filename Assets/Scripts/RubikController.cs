using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Google.Protobuf.WellKnownTypes;

public class CubePieceState
{
    public Vector3 solvedPosition;
    public Vector3 currentPosition;
    public Quaternion solvedRotation;
    public Quaternion currentRotation;
    public Dictionary<string, Color> faceColors;

    public CubePieceState(Transform piece)
    {
        solvedPosition = piece.localPosition;
        currentPosition = piece.localPosition;
        solvedRotation = piece.localRotation;
        currentRotation = piece.localRotation;
        faceColors = new Dictionary<string, Color>();
    }
}

public class CubeState
{
    public Dictionary<Transform, CubePieceState> pieceStates = new Dictionary<Transform, CubePieceState>();

    public CubeState(Transform cubeParent)
    {
        Transform[] pieces = cubeParent.GetComponentsInChildren<Transform>();
        foreach (Transform piece in pieces)
        {
            if (piece == cubeParent)
                continue;
            if (piece.gameObject.name == "Pivot")
                continue;
            pieceStates.Add(piece, new CubePieceState(piece));
        }
    }

    // Update the state to match the current positions/rotations of all pieces.
    public void UpdateState()
    {
        foreach (var kvp in pieceStates)
        {
            Transform piece = kvp.Key;
            CubePieceState state = kvp.Value;
            state.currentPosition = piece.localPosition;
            state.currentRotation = piece.localRotation;
        }
    }
}

public class RubikController : MonoBehaviour
{
    int n_size = 3;
    public int scrambleMovesCount = 20; // Number of random moves to scramble

    public enum RotationAxis { X, Y, Z }

    // Data structure for one rotation move.
    public class RotationMove
    {
        public RotationAxis axis;
        public int sliceIndex;
        public float angle;
        // Runtime fields set up during move execution.
        public GameObject pivot;
        public List<Transform> slicePieces;
        public Vector3 rotationAxis;
    }

    // Logical cube states.
    private CubeState solvedState;
    private CubeState currentState;

    public void StartCube()
    {
        // Retrieve the cube size (assumed to be set in a RubikGenerator component).
        n_size = GetComponent<RubikGenerator>().n_size;

        // Record the solved state before scrambling.
        solvedState = new CubeState(transform);
        currentState = new CubeState(transform);

        // Scramble the cube immediately.
        ScrambleCube();
    }

    // Scramble the cube by performing a series of instant moves.
    void ScrambleCube()
    {
        for (int i = 0; i < scrambleMovesCount; i++)
        {
            RotationMove move = new RotationMove();
            move.axis = (RotationAxis)Random.Range(0, 3);
            move.sliceIndex = Random.Range(0, n_size);
            // Randomly choose either 90° or -90°.
            move.angle = (Random.value > 0.5f) ? 90f : -90f;
            ExecuteRotationMove(move);
        }
        currentState.UpdateState();
        Debug.Log("Scramble complete. Correct Pieces: " + CountCorrectPieces());
    }

    // Called from (for example) a UI button to apply a single move.
    public void ApplyAction(int action)
    {
        RotationMove move = new RotationMove();
        switch (action)
        {
            case 0:
                move.axis = RotationAxis.X;
                move.sliceIndex = 0;
                move.angle = 90f;
                break;
            case 1:
                move.axis = RotationAxis.X;
                move.sliceIndex = 0;
                move.angle = -90f;
                break;
            case 2:
                move.axis = RotationAxis.X;
                move.sliceIndex = n_size - 1;
                move.angle = 90f;
                break;
            case 3:
                move.axis = RotationAxis.X;
                move.sliceIndex = n_size - 1;
                move.angle = -90f;
                break;
            case 4:
                move.axis = RotationAxis.Y;
                move.sliceIndex = 0;
                move.angle = 90f;
                break;
            case 5:
                move.axis = RotationAxis.Y;
                move.sliceIndex = 0;
                move.angle = -90f;
                break;
            case 6:
                move.axis = RotationAxis.Y;
                move.sliceIndex = n_size - 1;
                move.angle = 90f;
                break;
            case 7:
                move.axis = RotationAxis.Y;
                move.sliceIndex = n_size - 1;
                move.angle = -90f;
                break;
            case 8:
                move.axis = RotationAxis.Z;
                move.sliceIndex = 0;
                move.angle = 90f;
                break;
            case 9:
                move.axis = RotationAxis.Z;
                move.sliceIndex = 0;
                move.angle = -90f;
                break;
            case 10:
                move.axis = RotationAxis.Z;
                move.sliceIndex = n_size - 1;
                move.angle = 90f;
                break;
            case 11:
                move.axis = RotationAxis.Z;
                move.sliceIndex = n_size - 1;
                move.angle = -90f;
                break;
            default:
                Debug.LogError("Invalid action: " + action);
                return;
        }

        ExecuteRotationMove(move);
        currentState.UpdateState();
    }

    // Executes a rotation move instantly.
    void ExecuteRotationMove(RotationMove move)
    {
        SetupRotationMove(move);
        // Instantly apply the rotation.
        move.pivot.transform.Rotate(move.rotationAxis, move.angle, Space.Self);
        // Detach the pieces from the pivot.
        foreach (Transform piece in move.slicePieces)
        {
            piece.parent = transform;
        }
        Destroy(move.pivot);
    }

    // Prepares a rotation move by creating a pivot and reparenting the affected pieces.
    void SetupRotationMove(RotationMove move)
    {
        float tolerance = 0.1f;
        move.slicePieces = new List<Transform>();
        // Calculate the target coordinate for the slice.
        float targetCoord = move.sliceIndex - ((n_size - 1) / 2.0f);

        // Gather all pieces in the slice.
        foreach (Transform piece in transform)
        {
            if (piece.gameObject.name == "Pivot")
                continue;
            float coord = 0f;
            switch (move.axis)
            {
                case RotationAxis.X:
                    coord = piece.localPosition.x;
                    break;
                case RotationAxis.Y:
                    coord = piece.localPosition.y;
                    break;
                case RotationAxis.Z:
                    coord = piece.localPosition.z;
                    break;
            }
            if (Mathf.Abs(coord - targetCoord) < tolerance)
            {
                move.slicePieces.Add(piece);
            }
        }

        // Create the pivot at the correct position and orientation.
        move.pivot = new GameObject("Pivot");
        move.pivot.transform.parent = transform;
        switch (move.axis)
        {
            case RotationAxis.X:
                move.pivot.transform.localPosition = new Vector3(targetCoord, 0, 0);
                move.rotationAxis = Vector3.right;
                break;
            case RotationAxis.Y:
                move.pivot.transform.localPosition = new Vector3(0, targetCoord, 0);
                move.rotationAxis = Vector3.up;
                break;
            case RotationAxis.Z:
                move.pivot.transform.localPosition = new Vector3(0, 0, targetCoord);
                move.rotationAxis = Vector3.forward;
                break;
        }

        // Reparent the pieces to the pivot.
        foreach (Transform piece in move.slicePieces)
        {
            piece.parent = move.pivot.transform;
        }
    }

    // Example method: Counts how many pieces are in the correct (solved) state.
    public int CountCorrectPieces()
    {
        int count = 0;
        foreach (var kvp in currentState.pieceStates)
        {
            Transform piece = kvp.Key;
            CubePieceState currentPiece = kvp.Value;

            if (!solvedState.pieceStates.ContainsKey(piece))
                continue;
            CubePieceState solvedPiece = solvedState.pieceStates[piece];

            if (Vector3.Distance(currentPiece.currentPosition, solvedPiece.solvedPosition) < 0.01f &&
                Quaternion.Angle(currentPiece.currentRotation, solvedPiece.solvedRotation) < 1f)
            {
                count++;
            }
        }
        return count;
    }
}
