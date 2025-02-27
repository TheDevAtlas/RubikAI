using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RubiksCubeScanner : MonoBehaviour
{
    public Transform[] facePoints; // 6 transforms, one per face
    public float raycastDistance = 2f; // Distance to cube surface
    public LayerMask cubeLayer; // Layer for detecting cube pieces
    public Dictionary<string, Color> colorMapping; // Maps tag names to actual colors

    public Image[] uiGrids; // 54 UI Images representing the scanned cube

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

        // Start scanning
        ScanCube();
    }

    public void ScanCube()
    {
        List<Color> detectedColors = new List<Color>();

        for (int i = 0; i < facePoints.Length; i++)
        {
            detectedColors.AddRange(ScanFace(facePoints[i]));
        }

        UpdateUI(detectedColors);
    }

    private List<Color> ScanFace(Transform facePoint)
    {
        List<Color> faceColors = new List<Color>();

        // Offsets for 9 positions per face (3x3 grid, no diagonals)
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
                }
                else
                {
                    faceColors.Add(Color.black); // Default if no tag is found
                }
            }
            else
            {
                faceColors.Add(Color.black); // If raycast fails
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
