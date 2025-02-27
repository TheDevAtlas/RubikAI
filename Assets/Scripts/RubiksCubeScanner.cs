using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RubiksCubeScanner : MonoBehaviour
{
    [Header("Observation Settings")]
    public Transform[] facePoints; // 6 transforms, one per face
    public float raycastDistance = 2f; // Distance to cube surface
    public LayerMask cubeLayer; // Layer for detecting cube pieces

    public Image[] uiGrids; // 54 UI Images representing the scanned cube

    // Mapping from tag names to actual colors
    public Dictionary<string, Color> colorMapping;
    // Mapping from tag names to int indices (0-5)
    private Dictionary<string, int> colorIndexMapping;

    [Header("Observation Variables")]
    // List of int indices for ML agents. This list will be updated along with the UI colors.
    public List<int> mlColorIndices;
    // Original cube state (54 ints) captured at initialization.
    public List<int> originalCubeState;

    // Booleans for each face solved (one side has the same color)
    public bool face1Solved;
    public bool face2Solved;
    public bool face3Solved;
    public bool face4Solved;
    public bool face5Solved;
    public bool face6Solved;

    // Boolean for entire cube solved (all faces solved)
    public bool cubeSolved;

    [Header("Correct Piece Counts (per face)")]
    // These ints show how many pieces on each face match the center (correct) color.
    public int face1CorrectCount;
    public int face2CorrectCount;
    public int face3CorrectCount;
    public int face4CorrectCount;
    public int face5CorrectCount;
    public int face6CorrectCount;

    private void Start()
    {
        // Initialize color mapping (Change these to match your game)
        colorMapping = new Dictionary<string, Color>
        {
            { "White", Color.white },
            { "Blue", Color.blue },
            { "Red", Color.red },
            { "Green", Color.green },
            { "Orange", new Color(1f, 0.5f, 0f) }, // Orange color
            { "Yellow", Color.yellow }
        };

        // Initialize int mapping for ML (0-5 for each color)
        colorIndexMapping = new Dictionary<string, int>
        {
            { "White", 0 },
            { "Blue", 1 },
            { "Red", 2 },
            { "Green", 3 },
            { "Orange", 4 },
            { "Yellow", 5 }
        };

        // Perform initial scan
        ScanCube();
        // Save the initial (correct) configuration for comparison later.
        originalCubeState = new List<int>(mlColorIndices);
    }

    public void ScanCube()
    {
        List<Color> detectedColors = new List<Color>();
        mlColorIndices = new List<int>(); // Reset ML indices

        // Scan each face and combine the results
        for (int i = 0; i < facePoints.Length; i++)
        {
            List<int> faceColorInts;
            List<Color> faceColors = ScanFace(facePoints[i], out faceColorInts);
            detectedColors.AddRange(faceColors);
            mlColorIndices.AddRange(faceColorInts);
        }

        UpdateUI(detectedColors);
        CheckSolved();
        UpdateCorrectCounts();
    }

    // Updated ScanFace method now returns both colors and corresponding int indices.
    private List<Color> ScanFace(Transform facePoint, out List<int> faceColorInts)
    {
        List<Color> faceColors = new List<Color>();
        faceColorInts = new List<int>();

        // Offsets for 9 positions per face (3x3 grid)
        Vector3[] offsets = {
            new Vector3(-1f, 1f, 0), new Vector3(0, 1f, 0), new Vector3(1f, 1f, 0),
            new Vector3(-1f, 0, 0), new Vector3(0, 0, 0), new Vector3(1f, 0, 0),
            new Vector3(-1f, -1f, 0), new Vector3(0, -1f, 0), new Vector3(1f, -1f, 0)
        };

        Vector3 direction = -facePoint.forward; // Point towards the cube center

        for (int j = 0; j < offsets.Length; j++)
        {
            Vector3 rayOrigin = facePoint.position + facePoint.right * offsets[j].x + facePoint.up * offsets[j].y;
            if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, raycastDistance, cubeLayer))
            {
                string detectedTag = hit.collider.tag;
                if (colorMapping.TryGetValue(detectedTag, out Color detectedColor))
                {
                    faceColors.Add(detectedColor);
                    // Map the tag to an int value for ML agents
                    if (colorIndexMapping.TryGetValue(detectedTag, out int detectedIndex))
                    {
                        faceColorInts.Add(detectedIndex);
                    }
                    else
                    {
                        faceColorInts.Add(-1); // Unknown tag
                    }
                }
                else
                {
                    faceColors.Add(Color.black);
                    faceColorInts.Add(-1);
                }
            }
            else
            {
                faceColors.Add(Color.black);
                faceColorInts.Add(-1);
            }
        }

        return faceColors;
    }

    private void UpdateUI(List<Color> detectedColors)
    {
        for (int i = 0; i < uiGrids.Length && i < detectedColors.Count; i++)
        {
            uiGrids[i].color = detectedColors[i];
        }
    }

    // Check if each face is solved and if the whole cube is solved
    private void CheckSolved()
    {
        // Each face is 9 elements in mlColorIndices, total should be 54.
        if (mlColorIndices.Count != 54)
        {
            Debug.LogWarning("Incomplete cube scan.");
            return;
        }

        face1Solved = IsFaceSolved(0);
        face2Solved = IsFaceSolved(1);
        face3Solved = IsFaceSolved(2);
        face4Solved = IsFaceSolved(3);
        face5Solved = IsFaceSolved(4);
        face6Solved = IsFaceSolved(5);

        // Entire cube is solved if all faces are solved
        cubeSolved = face1Solved && face2Solved && face3Solved && face4Solved && face5Solved && face6Solved;
    }

    // Check if the face at the given index (0 to 5) is solved (all 9 pieces the same)
    private bool IsFaceSolved(int faceIndex)
    {
        int start = faceIndex * 9;
        int end = start + 9;

        // All elements in the face must be equal and not be -1.
        int first = mlColorIndices[start];
        if (first == -1)
            return false;

        for (int i = start + 1; i < end; i++)
        {
            if (mlColorIndices[i] != first)
                return false;
        }
        return true;
    }

    // Update the count of correctly placed pieces on each face.
    // The center piece is considered the "correct" color.
    private void UpdateCorrectCounts()
    {
        if (mlColorIndices.Count != 54)
        {
            Debug.LogWarning("Incomplete cube scan for correct counts.");
            return;
        }

        face1CorrectCount = CountCorrectForFace(0);
        face2CorrectCount = CountCorrectForFace(1);
        face3CorrectCount = CountCorrectForFace(2);
        face4CorrectCount = CountCorrectForFace(3);
        face5CorrectCount = CountCorrectForFace(4);
        face6CorrectCount = CountCorrectForFace(5);
    }

    // For a given face (0-5), count how many pieces match the center's color.
    private int CountCorrectForFace(int faceIndex)
    {
        int start = faceIndex * 9;
        int centerValue = mlColorIndices[start + 4]; // Center of the 3x3 face
        if (centerValue == -1)
            return 0;

        int count = 0;
        for (int i = start; i < start + 9; i++)
        {
            if (mlColorIndices[i] == centerValue)
                count++;
        }
        return count;
    }

    private void OnDrawGizmos()
    {
        if (facePoints == null) return;

        foreach (var facePoint in facePoints)
        {
            if (facePoint == null) continue;

            Vector3 direction = -facePoint.forward;
            Vector3[] offsets = {
                new Vector3(-1f, 1f, 0), new Vector3(0, 1f, 0), new Vector3(1f, 1f, 0),
                new Vector3(-1f, 0, 0), new Vector3(0, 0, 0), new Vector3(1f, 0, 0),
                new Vector3(-1f, -1f, 0), new Vector3(0, -1f, 0), new Vector3(1f, -1f, 0)
            };

            foreach (var offset in offsets)
            {
                Vector3 rayOrigin = facePoint.position + facePoint.right * offset.x + facePoint.up * offset.y;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(rayOrigin, rayOrigin + direction * raycastDistance);
            }
        }
    }
}
