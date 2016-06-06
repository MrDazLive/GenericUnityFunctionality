using UnityEngine;
using System.Threading;
using System.Collections.Generic;

public class MarchingTerrain : MonoBehaviour {

    private struct Node
    {
        public bool enabled;
        public Vector3 position;
    }

    [SerializeField, Range(0.1f, 10.0f)]
    private float meschScale = 1.0f;

    [SerializeField]
    private Vector3 dimensions = new Vector3(128.0f, 10.0f, 128.0f);
    private int dimensionCap = 64;

    [SerializeField]
    private bool invertPerlin = false;
    [SerializeField, Range(0.0f, 1.0f)]
    private float perlinDensity = 0.25f;
    [SerializeField, Range(1, 128)]
    private int perlinScale = 32;

    private Node[][] node;
    private MeshFilter[][] patch;

    private Vector2 patchDimensions = new Vector2();
    private Vector2 nodeDimensions = new Vector2();

    public void Start()
    {
        producePatches();
        produceNodes();
        produceMeshes();
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Start();
        }
    }

    private void producePatches()
    {
        foreach(Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }

        patchDimensions = new Vector2();
        patchDimensions.x = (int)Mathf.Ceil(dimensions.x / meschScale / dimensionCap);
        patchDimensions.y = (int)Mathf.Ceil(dimensions.z / meschScale / dimensionCap);

        patch = new MeshFilter[(int)patchDimensions.x][];
        for (int i = 0; i < (int)patchDimensions.x; i++)
        {
            patch[i] = new MeshFilter[(int)patchDimensions.y];
            for (int j = 0; j < (int)patchDimensions.y; j++)
            {
                GameObject c = new GameObject();
                c.transform.parent = gameObject.transform;
                c.AddComponent<MeshFilter>();
                c.AddComponent<MeshRenderer>();
                patch[i][j] = c.GetComponent<MeshFilter>();
            }
        }
    }

    private void produceNodes()
    {
        nodeDimensions.x = (int)Mathf.Ceil(dimensions.x / meschScale) + 1;
        nodeDimensions.y = (int)Mathf.Ceil(dimensions.z / meschScale) + 1;

        node = new Node[(int)nodeDimensions.x][];
        for (int i = 0; i < (int)nodeDimensions.x; i++)
        {
            node[i] = new Node[(int)nodeDimensions.y];
            float x = i * meschScale;
            for (int j = 0; j < (int)nodeDimensions.y; j++)
            {
                float y = j * meschScale;
                float perlin = Mathf.PerlinNoise(
                    (float)i / perlinScale + transform.position.x,
                    (float)j / perlinScale + transform.position.z);
                node[i][j] = new Node();
                node[i][j].position = new Vector3(x, perlin * dimensions.y, y);
                node[i][j].position += transform.position;

                node[i][j].enabled = invertPerlin ?
                    perlin < perlinDensity :
                    perlin >= perlinDensity;
            }
        }
    }

    private void produceMeshes()
    {
        nodeDimensions.x = (int)Mathf.Ceil(dimensions.x / meschScale);
        nodeDimensions.y = (int)Mathf.Ceil(dimensions.z / meschScale);

        for (int i = 0; i < (int)patchDimensions.x; i++)
        {
            for (int j = 0; j < (int)patchDimensions.y; j++)
            {
                MarchingMesh m = produceMesh(i, j);

                Mesh mesh = new Mesh();
                mesh.vertices = m.vertices.ToArray();
                mesh.triangles = m.triangles.ToArray();
                mesh.RecalculateNormals();
                patch[i][j].mesh = mesh;
            }
        }
    }

    private MarchingMesh produceMesh(int i, int j)
    {
        MarchingMesh m = new MarchingMesh();
        m.vertices = new List<Vector3>();
        m.triangles = new List<int>();

        int rowStart = i * dimensionCap;
        int rowCap = rowStart + dimensionCap;
        for (int x = rowStart; x < rowCap && x < (int)nodeDimensions.x; x++)
        {
            int colStart = j * dimensionCap;
            int colCap = colStart + dimensionCap;
            for (int y = colStart; y < colCap && y < (int)nodeDimensions.y; y++)
            {
                MarchingMesh mm = MarchingSquare<Node>.GetMesh(new Node[4] {
                                node[x][y],
                                node[x + 1][y],
                                node[x + 1][y + 1],
                                node[x][y + 1]
                            },
                    (n) => { return n.position; },
                    (n) => { return n.enabled; }
                );
                int offset = m.vertices.Count;
                for (int k = 0; k < mm.triangles.Count; k++)
                {
                    mm.triangles[k] += offset;
                }
                m.vertices.AddRange(mm.vertices);
                m.triangles.AddRange(mm.triangles);
            }
        }

        return m;
    }
}
