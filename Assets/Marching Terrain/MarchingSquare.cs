using UnityEngine;
using System;
using System.Collections.Generic;

public static class MarchingSquare<T>
{
    private static int meshId;
    private static MarchingMesh mesh;

    private static T[] corner;
    private static Func<T, bool> isCornerEnabled;
    private static Func<T, Vector3> getCornerPosition;

    private static int[][] triangle;
    private static Vector3[] vertex;

    public static MarchingMesh GetMesh(T[] corners, Func<T, Vector3> position, Func<T, bool> enabled)
    {
        mesh = new MarchingMesh();
        meshId = 0;

        corner = corners;
        isCornerEnabled = enabled;
        getCornerPosition = position;

        setUpVertices();
        setUpTriangles();

        detectCorners();
        detectConnections();

        produceMesh();

        return mesh;
    }

    private static void setUpVertices()
    {
        Vector3[] vert = new Vector3[4];

        vert[0] = getCornerPosition(corner[0]);
        vert[1] = getCornerPosition(corner[1]);
        vert[2] = getCornerPosition(corner[2]);
        vert[3] = getCornerPosition(corner[3]);

        vertex = new Vector3[8];

        vertex[0] = vert[0];
        vertex[1] = (vert[0] + vert[1]) / 2;
        vertex[2] = vert[1];
        vertex[3] = (vert[1] + vert[2]) / 2;
        vertex[4] = vert[2];
        vertex[5] = (vert[2] + vert[3]) / 2;
        vertex[6] = vert[3];
        vertex[7] = (vert[3] + vert[0]) / 2;
    }

    private static void setUpTriangles()
    {
        triangle = new int[4][];

        triangle[0] = new int[3];
        triangle[0][0] = 7;
        triangle[0][1] = 0;
        triangle[0][2] = 1;

        triangle[1] = new int[3];
        triangle[1][0] = 1;
        triangle[1][1] = 2;
        triangle[1][2] = 3;

        triangle[2] = new int[3];
        triangle[2][0] = 3;
        triangle[2][1] = 4;
        triangle[2][2] = 5;

        triangle[3] = new int[3];
        triangle[3][0] = 5;
        triangle[3][1] = 6;
        triangle[3][2] = 7;
    }

    private static void detectCorners()
    {
        mesh.vertices = new List<Vector3>();
        for (int i = 0; i < 4; i++)
        {
            if (isCornerEnabled(corner[i]))
            {
                meshId += (int)Mathf.Pow(2, i);
                for (int j = 0; j < 3; j++)
                {
                    mesh.vertices.Add(vertex[triangle[i][j]]);
                }
            }
        }
    }

    private static void detectConnections()
    {
        for (int k = 0; k < mesh.vertices.Count; k++)
        {
            for (int l = k + 1; l < mesh.vertices.Count; l++)
            {
                if (mesh.vertices[k] == mesh.vertices[l])
                {
                    Vector3 rm = mesh.vertices[k];
                    mesh.vertices.Remove(rm);
                    mesh.vertices.Remove(rm);
                    break;
                }
            }
        }
    }
    
    private static void produceMesh()
    {
        mesh.triangles = new List<int>();
        switch (meshId)
        {
            case 0:
                break;
            case 1:
            case 2:
            case 4:
            case 8:
                mesh.triangles.Add(2);
                mesh.triangles.Add(1);
                mesh.triangles.Add(0);
                break;
            case 3:
            case 6:
            case 9:
            case 12:
                mesh.triangles.Add(0);
                mesh.triangles.Add(2);
                mesh.triangles.Add(1);
                mesh.triangles.Add(0);
                mesh.triangles.Add(3);
                mesh.triangles.Add(2);
                break;
            case 5:
            case 10:
                mesh.triangles.Add(2);
                mesh.triangles.Add(1);
                mesh.triangles.Add(0);
                mesh.triangles.Add(5);
                mesh.triangles.Add(4);
                mesh.triangles.Add(3);
                mesh.triangles.Add(0);
                mesh.triangles.Add(5);
                mesh.triangles.Add(2);
                mesh.triangles.Add(5);
                mesh.triangles.Add(3);
                mesh.triangles.Add(2);
                break;
            case 7:
            case 11:
            case 13:
            case 14:
                mesh.triangles.Add(0);
                mesh.triangles.Add(2);
                mesh.triangles.Add(1);
                mesh.triangles.Add(0);
                mesh.triangles.Add(3);
                mesh.triangles.Add(2);
                mesh.triangles.Add(0);
                mesh.triangles.Add(4);
                mesh.triangles.Add(3);
                break;
            case 15:
                mesh.triangles.Add(0);
                mesh.triangles.Add(2);
                mesh.triangles.Add(1);
                mesh.triangles.Add(0);
                mesh.triangles.Add(3);
                mesh.triangles.Add(2);
                break;
        }
    }
}