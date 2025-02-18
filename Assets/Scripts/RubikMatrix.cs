using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class RubikMatrix : MonoBehaviour
{
    int n_size = 3;
    public int[,,] rubiksCube;
    public int[,,] solvedRubiksCube;

    private void Start()
    {
        // Create a new NxNxN Rubik's Cube //
        rubiksCube = new int[n_size, n_size, n_size];
        solvedRubiksCube = new int[n_size, n_size, n_size];

        // Init values for the Rubik's Cube //
        // For a 3x3x3 Cube, 14 is the center //
        int i = 1;
        for (int x = 0; x < n_size; x++)
        {
            for (int y = 0; y < n_size; y++)
            {
                for (int z = 0; z < n_size; z++)
                {
                    rubiksCube[x, y, z] = i;
                    solvedRubiksCube[x, y, z] = i;
                    i++;
                }
            }
        }

        // Scramble cube //
        ScrambleCube();

        // Check how many pieces correct //
        CheckSolution();
    }

    public void ScrambleCube()
    {
        // Test: Rotate only one side of the cube (+x side) //
        List<int> piecesToRotate = new List<int>();

        // Add the 8 pieces //
        piecesToRotate.Add(rubiksCube[0, 0, 0]);
        piecesToRotate.Add(rubiksCube[1, 0, 0]);
        piecesToRotate.Add(rubiksCube[2, 0, 0]);
        piecesToRotate.Add(rubiksCube[2, 1, 0]);
        piecesToRotate.Add(rubiksCube[2, 2, 0]);
        piecesToRotate.Add(rubiksCube[1, 2, 0]);
        piecesToRotate.Add(rubiksCube[0, 2, 0]);
        piecesToRotate.Add(rubiksCube[0, 1, 0]);

        // Rotate //
        rubiksCube[0,0,0] = piecesToRotate[2];
        rubiksCube[1,0,0] = piecesToRotate[3];
        rubiksCube[2, 0, 0] = piecesToRotate[4];
        rubiksCube[2, 1, 0] = piecesToRotate[5];
        rubiksCube[2, 2, 0] = piecesToRotate[6];
        rubiksCube[1, 2, 0] = piecesToRotate[7];
        rubiksCube[0, 2, 0] = piecesToRotate[0];
        rubiksCube[0, 1, 0] = piecesToRotate[1];
    }

    public int CheckSolution()
    {
        // Loop through cube and check if it is the same as reference //
        int score = 1;
        for (int x = 0; x < n_size; x++)
        {
            for (int y = 0; y < n_size; y++)
            {
                for (int z = 0; z < n_size; z++)
                {
                    if(rubiksCube[x, y, z] == solvedRubiksCube[x, y, z])
                    {
                        score++;
                    }
                }
            }
        }

        // Write out score / if cube is solved //
        if(score == 28)
        {
            Debug.Log("Cube Solved! " + score);
        }
        else
        {
            Debug.Log("Score: " + score);
        }

        return score;
    }
}
