using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubiksCubeController : MonoBehaviour
{
    public Transform pieceHolder;
    public RubiksCubeScanner scanner;

    public float rotationDuration = 0.5f;
    public bool instantRotation = false;
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public int scrambleMoves = 20;

    public List<Transform> pieces = new List<Transform>();
    public Stack<(string, float)> moveHistory = new Stack<(string, float)>();
    public string[] faces = { "Top", "Bottom", "Left", "Right", "Front", "Back" };

    private void Start()
    {
        //foreach (Transform piece in pieceHolder)
        //{
        //    pieces.Add(piece);
        //}

        //StartCoroutine(Scramble(scrambleMoves));
    }

    public IEnumerator Scramble(int moves)
    {
        for (int i = 0; i < moves; i++)
        {
            string face = faces[Random.Range(0, faces.Length)];
            float angle = Random.Range(0, 2) == 0 ? 90f : -90f;
            moveHistory.Push((face, -angle));
            yield return RotateFace(face, angle, true);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator RotateFace(string face, float targetAngle, bool isAccelerated)
    {
        float mod = isAccelerated ? 0.25f : 1f;

        Transform facePivot = new GameObject(face + " Pivot").transform;
        facePivot.SetParent(transform, false);

        List<Transform> facePieces = GetFacePieces(face);
        foreach (Transform piece in facePieces)
        {
            piece.SetParent(facePivot, true);
        }

        if (instantRotation)
        {
            facePivot.localRotation *= Quaternion.Euler(GetRotationAxis(face) * targetAngle);
        }
        else
        {
            float elapsedTime = 0f;
            float lastCurveValue = 0f;

            while (elapsedTime < (rotationDuration * mod))
            {
                float t = elapsedTime / (rotationDuration * mod);
                float curveValue = rotationCurve.Evaluate(t);

                float deltaAngle = (curveValue - lastCurveValue) * targetAngle;
                lastCurveValue = curveValue;

                facePivot.localRotation *= Quaternion.Euler(GetRotationAxis(face) * deltaAngle);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            facePivot.localRotation = Quaternion.Euler(GetRotationAxis(face) * targetAngle);
        }

        foreach (Transform piece in facePieces)
        {
            piece.SetParent(transform, true);
        }

        Destroy(facePivot.gameObject);

        yield return new WaitForSeconds(0.1f);

        scanner.ScanCube();
    }

    private List<Transform> GetFacePieces(string face)
    {
        List<Transform> facePieces = new List<Transform>();

        foreach (Transform piece in pieces)
        {
            Vector3 localPos = piece.localPosition;
            switch (face)
            {
                case "Top":
                    if (Mathf.Abs(localPos.y - 1f) < 0.1f) facePieces.Add(piece);
                    break;
                case "Bottom":
                    if (Mathf.Abs(localPos.y + 1f) < 0.1f) facePieces.Add(piece);
                    break;
                case "Left":
                    if (Mathf.Abs(localPos.x - 1f) < 0.1f) facePieces.Add(piece);
                    break;
                case "Right":
                    if (Mathf.Abs(localPos.x + 1f) < 0.1f) facePieces.Add(piece);
                    break;
                case "Front":
                    if (Mathf.Abs(localPos.z - 1f) < 0.1f) facePieces.Add(piece);
                    break;
                case "Back":
                    if (Mathf.Abs(localPos.z + 1f) < 0.1f) facePieces.Add(piece);
                    break;
            }
        }

        return facePieces;
    }

    private Vector3 GetRotationAxis(string face)
    {
        switch (face)
        {
            case "Top":
            case "Bottom":
                return Vector3.up;
            case "Left":
            case "Right":
                return Vector3.right;
            case "Front":
            case "Back":
                return Vector3.forward;
            default:
                return Vector3.zero;
        }
    }
}
