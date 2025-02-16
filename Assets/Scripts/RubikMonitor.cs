using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RubikMonitor : MonoBehaviour
{
    [Header("Prefab Settings")]
    [Tooltip("Prefab that contains the RubikGenerator and RubikController components.")]
    public GameObject rubikCubePrefab;

    [Header("Training Settings")]
    [Tooltip("Maximum moves per episode.")]
    public int maxMoves = 50;
    [Tooltip("Total number of training iterations (episodes) to run.")]
    public int iterations = 1000;
    
    [Header("Checkpoint Settings")]
    [Tooltip("Optional JSON string containing a checkpoint to load network weights from.")]
    [TextArea]
    public string checkpointJson;
    [Tooltip("Path to save the network weights JSON after training.")]
    public string saveFilePath = "rubik_weights.json";
    [Tooltip("If true, saves the network weights after training.")]
    public bool saveResults = true;

    // The RL agent that will “learn” to solve the cube.
    private RubiksRLAgent agent;

    // Reference to the currently active Rubik's cube instance.
    private GameObject currentCubeInstance;

    private void Start()
    {
        // Spawn the Rubik's cube prefab (which should auto-generate and scramble itself).
        SpawnCube();

        // Create and initialize the RL agent.
        agent = new RubiksRLAgent();
        if (!string.IsNullOrEmpty(checkpointJson))
        {
            agent.LoadWeightsFromJson(checkpointJson);
            Debug.Log("Loaded checkpoint weights from JSON.");
        }
        else
        {
            agent.InitializeWeights();
            Debug.Log("Initialized new random network weights.");
        }

        // Start the training coroutine.
        StartCoroutine(TrainAgent());
    }

    /// <summary>
    /// Spawns (or respawns) the Rubik's cube prefab.
    /// </summary>
    private void SpawnCube()
    {
        if (currentCubeInstance != null)
        {
            Destroy(currentCubeInstance);
        }
        currentCubeInstance = Instantiate(rubikCubePrefab);
    }

    /// <summary>
    /// Trains the RL agent for the given number of iterations.
    /// Each episode starts with a freshly spawned (and scrambled) cube.
    /// </summary>
    private IEnumerator TrainAgent()
    {
        for (int epoch = 0; epoch < iterations; epoch++)
        {
            // Spawn a new cube environment for this episode.
            SpawnCube();
            RubikController controller = currentCubeInstance.GetComponent<RubikController>();

            // (Assume the cube scrambles itself on Start.)
            // Wait a short time to allow the scramble animation (if any) to complete.
            yield return new WaitForSeconds(1f);

            float episodeReward = 0f;
            // Dummy state from the cube (in a full version you’d encode the cube’s configuration)
            float[] state = agent.GetStateFromCube();

            for (int moveCount = 0; moveCount < maxMoves; moveCount++)
            {
                // Choose an action (an integer in [0, 11]: 6 faces × 2 directions)
                int action = agent.ChooseAction(state);

                // Apply the action to the cube.
                // NOTE: You must implement ApplyAction(int action) in your RubikController
                // (or extend it via a wrapper) so that the chosen move is executed.
                controller.ApplyAction(action);

                // Wait for the move to finish (using your controller’s timing)
                yield return new WaitForSeconds(controller.rotationDuration + controller.waitTime);

                // Get the new state from the cube.
                float[] nextState = agent.GetStateFromCube();

                // Compute a reward (in a full implementation, reward should reflect how “solved” the cube is)
                float reward = agent.ComputeReward(state, action, nextState);
                episodeReward += reward;

                // Update the agent’s network based on the transition.
                agent.Learn(state, action, reward, nextState);

                // For this dummy example, we check if the cube is "solved"
                if (agent.IsSolved(nextState))
                {
                    Debug.Log("Cube solved in epoch " + epoch + " after " + (moveCount + 1) + " moves.");
                    break;
                }

                // Prepare for the next move.
                state = nextState;
            }

            Debug.Log("Epoch " + epoch + " complete with total reward: " + episodeReward);

            // (Optional) Pause a bit between episodes.
            yield return new WaitForSeconds(0.5f);
        }

        // After training, optionally save the network weights.
        if (saveResults)
        {
            string json = agent.SaveWeightsToJson();
            File.WriteAllText(saveFilePath, json);
            Debug.Log("Saved network weights to " + saveFilePath);
        }
    }

    /// <summary>
    /// A dummy reinforcement learning agent.
    /// In a real implementation you would use a proper state encoding and update rules.
    /// </summary>
    public class RubiksRLAgent
    {
        // For demonstration, we assume there are 12 possible actions.
        public int numActions = 12;

        // Our network weights (here simply represented as a float array).
        public float[] weights;

        /// <summary>
        /// Initializes the network weights randomly.
        /// </summary>
        public void InitializeWeights()
        {
            // For example, we use 100 weights (adjust as needed).
            weights = new float[100];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = Random.Range(-1f, 1f);
            }
        }

        /// <summary>
        /// Returns a dummy state from the cube.
        /// In practice, encode the cube's configuration into a float[].
        /// </summary>
        public float[] GetStateFromCube()
        {
            // Dummy state: array of 10 floats.
            float[] state = new float[10];
            for (int i = 0; i < state.Length; i++)
            {
                state[i] = Random.value;
            }
            return state;
        }

        /// <summary>
        /// Chooses an action given the current state.
        /// (For demonstration, this picks a random action.)
        /// </summary>
        public int ChooseAction(float[] state)
        {
            // In a full implementation, use the state and network weights to select an action (e.g., epsilon-greedy).
            return Random.Range(0, numActions);
        }

        /// <summary>
        /// Computes a dummy reward based on the transition.
        /// In practice, you would measure how much closer the cube is to being solved.
        /// </summary>
        public float ComputeReward(float[] state, int action, float[] nextState)
        {
            // Dummy reward: random value (or a heuristic based on state difference).
            return Random.Range(0f, 1f);
        }

        /// <summary>
        /// Updates the network weights given a state transition.
        /// (This is just a placeholder; replace with your learning algorithm.)
        /// </summary>
        public void Learn(float[] state, int action, float reward, float[] nextState)
        {
            // Dummy learning: tweak one weight (this is just illustrative).
            int index = action % weights.Length;
            weights[index] += reward * 0.01f;
        }

        /// <summary>
        /// Checks if the cube is “solved” based on the state.
        /// Replace with your own termination condition.
        /// </summary>
        public bool IsSolved(float[] state)
        {
            // Dummy check: in a real scenario, check if the cube’s state meets the solved condition.
            return false;
        }

        /// <summary>
        /// Loads network weights from a JSON string.
        /// </summary>
        public void LoadWeightsFromJson(string json)
        {
            WeightsData data = JsonUtility.FromJson<WeightsData>(json);
            weights = data.weights;
        }

        /// <summary>
        /// Saves network weights to a JSON string.
        /// </summary>
        public string SaveWeightsToJson()
        {
            WeightsData data = new WeightsData { weights = this.weights };
            return JsonUtility.ToJson(data, true);
        }

        [System.Serializable]
        public class WeightsData
        {
            public float[] weights;
        }
    }
}
