using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RubikAgent : Agent
{
    [Tooltip("Reference to the RubikController component.")]
    public RubikController rubikController;

    // If your RubikController is on the same GameObject, you can get it in Initialize.
    public override void Initialize()
    {
        if (rubikController == null)
        {
            rubikController = GetComponent<RubikController>();
        }
    }

    // Called at the beginning of each training episode.
    public override void OnEpisodeBegin()
    {
        // Reset the environment.
        // Option 1: If you have a ResetCube method on your RubikController, call it here.
        // rubikController.ResetCube();

        // Option 2: Otherwise, simply reload the scene.
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Collect observations from the environment.
    public override void CollectObservations(VectorSensor sensor)
    {
        // For simplicity, we observe the fraction of pieces that are correctly placed.
        // (Assumes a standard 3x3x3 Rubik's Cube; adjust if necessary.)
        int correctPieces = rubikController.CountCorrectPieces();
        int totalPieces = 27;
        sensor.AddObservation(correctPieces / (float)totalPieces);
    }

    // Called when the agent receives an action.
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // The action is a discrete integer in the range [0, 11].
        int action = actionBuffers.DiscreteActions[0];

        // Apply the move instantly.
        rubikController.ApplyAction(action);

        // Simple reward: fraction of pieces in the correct position.
        int correctPieces = rubikController.CountCorrectPieces();
        int totalPieces = 27;
        float reward = correctPieces / (float)totalPieces;
        SetReward(reward);

        // End the episode if the cube is solved.
        if (correctPieces == totalPieces)
        {
            SetReward(1.0f);
            EndEpisode();
        }
    }

    // Optional: Allows manual control (useful for testing).
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        // Map keys 0-9 and Q, E to the 12 possible moves.
        if (Input.GetKeyDown(KeyCode.Alpha0)) { discreteActions[0] = 0; }
        else if (Input.GetKeyDown(KeyCode.Alpha1)) { discreteActions[0] = 1; }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { discreteActions[0] = 2; }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) { discreteActions[0] = 3; }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) { discreteActions[0] = 4; }
        else if (Input.GetKeyDown(KeyCode.Alpha5)) { discreteActions[0] = 5; }
        else if (Input.GetKeyDown(KeyCode.Alpha6)) { discreteActions[0] = 6; }
        else if (Input.GetKeyDown(KeyCode.Alpha7)) { discreteActions[0] = 7; }
        else if (Input.GetKeyDown(KeyCode.Alpha8)) { discreteActions[0] = 8; }
        else if (Input.GetKeyDown(KeyCode.Alpha9)) { discreteActions[0] = 9; }
        else if (Input.GetKeyDown(KeyCode.Q)) { discreteActions[0] = 10; }
        else if (Input.GetKeyDown(KeyCode.E)) { discreteActions[0] = 11; }
        else { discreteActions[0] = 0; }
    }
}
