using System.Collections.Generic;
using UnityEngine;

//------------------------------------------------------------------------------
// This class stores the state of a single cube piece.
// You can extend it (for example, to include face colors) as needed.
public class CubePieceState {
    public Vector3 solvedPosition;
    public Vector3 currentPosition;
    public Quaternion solvedRotation;
    public Quaternion currentRotation;
    // Optionally, include additional properties (like faceColors)
    public Dictionary<string, Color> faceColors;

    public CubePieceState(Transform piece) {
        solvedPosition = piece.localPosition;
        currentPosition = piece.localPosition;
        solvedRotation = piece.localRotation;
        currentRotation = piece.localRotation;
        faceColors = new Dictionary<string, Color>(); // Fill this in if needed.
    }
}

//------------------------------------------------------------------------------
// This class tracks the state of all the cube pieces.
// It gathers (and later updates) all pieces that are part of the cube.
public class CubeState {
    // We use the Transform as the key (you might assign a unique ID instead)
    public Dictionary<Transform, CubePieceState> pieceStates = new Dictionary<Transform, CubePieceState>();

    public CubeState(Transform cubeParent) {
        // Get all child transforms (recursively) but skip the parent and any temporary pivots.
        Transform[] pieces = cubeParent.GetComponentsInChildren<Transform>();
        foreach (Transform piece in pieces) {
            if (piece == cubeParent)
                continue;
            if (piece.gameObject.name == "Pivot")
                continue;
            pieceStates.Add(piece, new CubePieceState(piece));
        }
    }

    // Call this after a move is complete to update the current positions/rotations.
    public void UpdateState() {
        foreach (var kvp in pieceStates) {
            Transform piece = kvp.Key;
            CubePieceState state = kvp.Value;
            state.currentPosition = piece.localPosition;
            state.currentRotation = piece.localRotation;
        }
    }
}

//------------------------------------------------------------------------------
// This is your main Rubik's Cube controller.
// It not only rotates slices but also keeps a logical record of the cube’s state.
public class RubikController : MonoBehaviour
{
    int n_size = 3;
    public float rotationDuration = 0.3f;
    public float waitTime = 0.8f;
    public int scrambleMovesCount = 20; // Number of random moves to scramble

    public enum RotationAxis { X, Y, Z }

    // Internal state for our non-coroutine state machine.
    private enum RotationState { Idle, Waiting, Rotating }
    private RotationState state = RotationState.Idle;
    private float stateStartTime = 0f;
    private int currentMoveIndex = 0;
    private RotationMove currentMove = null;
    private List<RotationMove> moves = new List<RotationMove>();

    // Data structure for one rotation move.
    public class RotationMove
    {
        public RotationAxis axis;
        public int sliceIndex;
        public float angle;
        public float duration;

        // These runtime fields are initialized when the move starts.
        public GameObject pivot;
        public List<Transform> slicePieces;
        public Vector3 rotationAxis;
        public float elapsed;
        public float currentAngle;
    }

    //--------------------------------------------------------------------------
    // Logical cube states: one for the solved (reference) state and one for the current state.
    private CubeState solvedState;
    private CubeState currentState;

    void Start()
    {
        // Get the cube size from the RubikGenerator.
        n_size = GetComponent<RubikGenerator>().n_size;

        // Record the solved state BEFORE scrambling.
        solvedState = new CubeState(transform);
        // Initially, the current state is the same as the solved state.
        currentState = new CubeState(transform);

        // Now scramble the cube.
        ScrambleCube();
    }

    // Generate a random scramble sequence.
    void ScrambleCube()
    {
        moves.Clear();
        for (int i = 0; i < scrambleMovesCount; i++)
        {
            RotationMove move = new RotationMove();
            move.axis = (RotationAxis)Random.Range(0, 3);
            move.sliceIndex = Random.Range(0, n_size);
            // Choose either 90° or -90° randomly.
            move.angle = (Random.value > 0.5f) ? 90f : -90f;
            move.duration = rotationDuration; // Ensure scramble moves have a duration.
            moves.Add(move);
        }
        currentMoveIndex = 0;
        state = RotationState.Waiting;
        stateStartTime = Time.time;
    }

    void Update()
    {
        // Waiting between moves.
        if (state == RotationState.Waiting)
        {
            if (Time.time - stateStartTime >= waitTime)
            {
                if (currentMoveIndex < moves.Count)
                {
                    currentMove = moves[currentMoveIndex];
                    SetupRotationMove(currentMove);
                    state = RotationState.Rotating;
                }
                else
                {
                    state = RotationState.Idle;
                    // Once idle, update the logical cube state.
                    currentState.UpdateState();

                    // Optionally, you can check progress. For example:
                    Debug.Log("Correct Pieces: " + CountCorrectPieces());
                }
            }
        }
        // Animating a rotation move.
        else if (state == RotationState.Rotating)
        {
            if (currentMove == null)
                return; // Safety check.

            currentMove.elapsed += Time.deltaTime;
            float fraction = Mathf.Clamp01(currentMove.elapsed / currentMove.duration);
            float targetAngle = Mathf.Lerp(0, currentMove.angle, fraction);
            float deltaAngle = targetAngle - currentMove.currentAngle;

            currentMove.pivot.transform.Rotate(currentMove.rotationAxis, deltaAngle, Space.Self);
            currentMove.currentAngle += deltaAngle;

            // When the rotation duration is complete, ensure the final angle is exact.
            if (currentMove.elapsed >= currentMove.duration)
            {
                float finalDelta = currentMove.angle - currentMove.currentAngle;
                currentMove.pivot.transform.Rotate(currentMove.rotationAxis, finalDelta, Space.Self);

                // Detach all pieces from the pivot and clean up.
                foreach (Transform piece in currentMove.slicePieces)
                {
                    piece.parent = transform;
                }
                Destroy(currentMove.pivot);

                currentMove = null;
                currentMoveIndex++;
                state = (currentMoveIndex < moves.Count) ? RotationState.Waiting : RotationState.Waiting;
                stateStartTime = Time.time;
            }
        }
    }

    //--------------------------------------------------------------------------
    // Prepares a rotation move by creating a pivot, gathering the slice's pieces,
    // and reparenting those pieces under the pivot.
    public void SetupRotationMove(RotationMove move)
    {
        float tolerance = 0.1f;
        move.slicePieces = new List<Transform>();
        float targetCoord = move.sliceIndex - ((n_size - 1) / 2.0f);

        // Find all pieces in the correct slice.
        foreach (Transform piece in transform)
        {
            // Skip any temporary pivot objects.
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

        // If no pieces are found, skip setting up this move.
        if (move.slicePieces.Count == 0)
            return;

        // Create a new pivot object and set its local position.
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

        move.elapsed = 0f;
        move.currentAngle = 0f;
    }

    //--------------------------------------------------------------------------
    // This method can be called (for example, from a UI button) to apply a single move.
    public void ApplyAction(int action)
    {
        // Only allow a new action if we're idle.
        if (state != RotationState.Idle)
        {
            Debug.LogWarning("Cube is busy; action skipped.");
            return;
        }

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

        move.duration = rotationDuration;

        // Clear any queued moves and start this one.
        moves.Clear();
        moves.Add(move);
        currentMoveIndex = 0;
        state = RotationState.Waiting;
        stateStartTime = Time.time;
    }

    //--------------------------------------------------------------------------
    // Example method: Count the number of pieces that match their solved state.
    // (Here we compare local positions and rotations with small tolerances.)
    public int CountCorrectPieces()
    {
        int count = 0;
        foreach (var kvp in currentState.pieceStates)
        {
            Transform piece = kvp.Key;
            CubePieceState currentPiece = kvp.Value;

            // Get the corresponding solved state.
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
