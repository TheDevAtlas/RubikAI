using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubikController : MonoBehaviour
{
    // Should match the RubikGenerator's n_size
    public int n_size = 3;

    // Duration for each 90° rotation (in seconds)
    public float rotationDuration = 0.3f;
    // Wait time between rotations (in seconds)
    public float waitTime = 0.8f;

    // Define an enum for clarity
    public enum RotationAxis { X, Y, Z }

    private void Start()
    {
        // Start the automated slice rotation demonstration
        StartCoroutine(RotateAllSlices());
    }

    /// <summary>
    /// Iterates over every slice (each axis and each index) and rotates it forward and back.
    /// </summary>
    IEnumerator RotateAllSlices()
    {
        yield return new WaitForSeconds(waitTime);
        // Loop through each axis (X, Y, Z)
        foreach (RotationAxis axis in System.Enum.GetValues(typeof(RotationAxis)))
        {
            // For each slice in the chosen axis
            for (int sliceIndex = 0; sliceIndex < n_size; sliceIndex++)
            {
                // Rotate 90 degrees in one direction
                yield return RotateSlice(axis, sliceIndex, 90f, rotationDuration);
                yield return new WaitForSeconds(waitTime);
                // Rotate back by -90 degrees
                yield return RotateSlice(axis, sliceIndex, -90f, rotationDuration);
                yield return new WaitForSeconds(waitTime);
            }
        }
    }

    /// <summary>
    /// Rotates a slice of cubies (children of this GameObject) that belong to the given axis and slice index.
    /// </summary>
    /// <param name="axis">Axis about which to rotate (X, Y, or Z)</param>
    /// <param name="sliceIndex">The index of the slice (0 to n_size - 1)</param>
    /// <param name="angle">The angle (in degrees) to rotate (positive or negative)</param>
    /// <param name="duration">How long the rotation should take</param>
    public IEnumerator RotateSlice(RotationAxis axis, int sliceIndex, float angle, float duration)
    {
        // Tolerance for matching a cubie's coordinate to the slice's coordinate
        float tolerance = 0.1f;
        List<Transform> slicePieces = new List<Transform>();

        // Calculate the expected coordinate along the chosen axis.
        // Note: The generator positions pieces at: localPosition = (x, y, z) - ((n_size-1)/2)
        float targetCoord = sliceIndex - ((n_size - 1) / 2.0f);

        // Loop through all direct children (each cubie)
        foreach (Transform piece in transform)
        {
            float coord = 0f;
            switch (axis)
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
                slicePieces.Add(piece);
            }
        }

        // If no pieces were found, exit early
        if (slicePieces.Count == 0)
            yield break;

        // Create a temporary pivot object that will be used to rotate the slice.
        GameObject pivot = new GameObject("Pivot");
        pivot.transform.parent = this.transform;
        // Set the pivot's local position so that it lies on the correct slice:
        switch (axis)
        {
            case RotationAxis.X:
                pivot.transform.localPosition = new Vector3(targetCoord, 0, 0);
                break;
            case RotationAxis.Y:
                pivot.transform.localPosition = new Vector3(0, targetCoord, 0);
                break;
            case RotationAxis.Z:
                pivot.transform.localPosition = new Vector3(0, 0, targetCoord);
                break;
        }

        // Parent each piece in the slice to the pivot
        foreach (Transform piece in slicePieces)
        {
            piece.parent = pivot.transform;
        }

        // Determine the axis vector (in the pivot's local space)
        Vector3 rotationAxis = Vector3.zero;
        switch (axis)
        {
            case RotationAxis.X:
                rotationAxis = Vector3.right;
                break;
            case RotationAxis.Y:
                rotationAxis = Vector3.up;
                break;
            case RotationAxis.Z:
                rotationAxis = Vector3.forward;
                break;
        }

        // Animate the rotation over the specified duration.
        float elapsed = 0f;
        float currentAngle = 0f;
        while (elapsed < duration)
        {
            // Calculate the incremental rotation for this frame.
            float deltaAngle = Mathf.Lerp(0, angle, elapsed / duration) - currentAngle;
            pivot.transform.Rotate(rotationAxis, deltaAngle, Space.Self);
            currentAngle += deltaAngle;
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Correct any small discrepancies
        float finalDelta = angle - currentAngle;
        pivot.transform.Rotate(rotationAxis, finalDelta, Space.Self);

        // Unparent all pieces back to the Rubik's cube
        foreach (Transform piece in slicePieces)
        {
            piece.parent = this.transform;
        }
        // Destroy the temporary pivot
        Destroy(pivot);
    }
}
