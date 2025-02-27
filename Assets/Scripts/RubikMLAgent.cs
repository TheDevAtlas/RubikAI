using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RubikMLAgent : Agent
{
    [Header("References")]
    // Reference to the Rubik's cube scanner that provides observations.
    public RubiksCubeScanner cubeScanner;
    // Reference to the Rubik's cube controller that performs moves.
    public RubiksCubeController cubeController;

    [Header("Settings")]
    public int scrambleMoves;

    public bool isRotating;

    public override void Initialize()
    {
        foreach (Transform piece in cubeController.pieceHolder)
        {
            cubeController.pieces.Add(piece);
        }
        base.Initialize();
    }

    // Reset the environment for a new episode //
    public override void OnEpisodeBegin()
    {
        cubeController.Scramble(scrambleMoves);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the current cube state.
        // The cubeScanner provides a list of 54 int values (0-5) representing the color indices of each piece.
        foreach (int colorIndex in cubeScanner.mlColorIndices)
        {
            sensor.AddObservation(colorIndex);
        }

        // Faces Solved, Or Cube Solved //
        sensor.AddObservation(cubeScanner.face1Solved);
        sensor.AddObservation(cubeScanner.face2Solved);
        sensor.AddObservation(cubeScanner.face3Solved);
        sensor.AddObservation(cubeScanner.face4Solved);
        sensor.AddObservation(cubeScanner.face5Solved);
        sensor.AddObservation(cubeScanner.face6Solved);
        sensor.AddObservation(cubeScanner.cubeSolved);

        // Counts For Correct Pieces //
        sensor.AddObservation(cubeScanner.face1CorrectCount);
        sensor.AddObservation(cubeScanner.face2CorrectCount);
        sensor.AddObservation(cubeScanner.face3CorrectCount);
        sensor.AddObservation(cubeScanner.face4CorrectCount);
        sensor.AddObservation(cubeScanner.face5CorrectCount);
        sensor.AddObservation(cubeScanner.face6CorrectCount);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // If a move is already underway, do not start a new one.
        if (isRotating)
            return;

        // Get the discrete action (an integer between 0 and 11).
        int action = actions.DiscreteActions[0];
        StartCoroutine(PerformMove(action));
    }

    private IEnumerator PerformMove(int action)
    {
        isRotating = true;

        // Map the action value to a face and rotation direction.
        // Actions 0-5: clockwise rotations; Actions 6-11: counterclockwise rotations.
        string[] faces = { "Top", "Bottom", "Left", "Right", "Front", "Back" };
        int faceIndex = action % 6;
        bool clockwise = action < 6;
        string face = faces[faceIndex];
        float angle = clockwise ? 90f : -90f;

        // Execute the move.
        // NOTE: This code assumes that RubiksCubeController has a public coroutine method RotateFace
        // that accepts (string face, float angle, bool isAccelerated).
        yield return StartCoroutine(cubeController.RotateFace(face, angle, false));

        // Allow a brief delay if needed (the controller also calls cubeScanner.ScanCube() at the end of a move).
        yield return new WaitForSeconds(0.1f);

        // Reward shaping: apply a small negative reward for each move to encourage efficiency.
        AddReward(-0.01f);

        // Add Reward If Getting Faces
        if (cubeScanner.face1CorrectCount >= 5) { AddReward(0.02f * cubeScanner.face1CorrectCount); }
        if (cubeScanner.face2CorrectCount >= 5) { AddReward(0.02f * cubeScanner.face2CorrectCount); }
        if (cubeScanner.face3CorrectCount >= 5) { AddReward(0.02f * cubeScanner.face3CorrectCount); }
        if (cubeScanner.face4CorrectCount >= 5) { AddReward(0.02f * cubeScanner.face4CorrectCount); }
        if (cubeScanner.face5CorrectCount >= 5) { AddReward(0.02f * cubeScanner.face5CorrectCount); }
        if (cubeScanner.face6CorrectCount >= 5) { AddReward(0.02f * cubeScanner.face6CorrectCount); }

        // Add Reward For Fully Solved Faces
        if (cubeScanner.face1Solved) { AddReward(0.2f); }
        if (cubeScanner.face2Solved) { AddReward(0.2f); }
        if (cubeScanner.face3Solved) { AddReward(0.2f); }
        if (cubeScanner.face4Solved) { AddReward(0.2f); }
        if (cubeScanner.face5Solved) { AddReward(0.2f); }
        if (cubeScanner.face6Solved) { AddReward(0.2f); }

        print("Adding Rewards");

        // Check if the cube is solved.
        if (cubeScanner.cubeSolved)
        {
            AddReward(1.0f);  // Large reward for solving the cube.
            EndEpisode();
        }

        isRotating = false;
    }
}
