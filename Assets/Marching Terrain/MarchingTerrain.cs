using UnityEngine;
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

        Vector2 dimension = new Vector2();
        dimension.x = (int)Mathf.Ceil(dimensions.x / meschScale / dimensionCap);
        dimension.y = (int)Mathf.Ceil(dimensions.z / meschScale / dimensionCap);

        patch = new MeshFilter[(int)dimension.x][];
        for (int i = 0; i < (int)dimension.x; i++)
        {
            patch[i] = new MeshFilter[(int)dimension.y];
            for (int j = 0; j < (int)dimension.y; j++)
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
        Vector2 dimension = new Vector2();
        dimension.x = (int)Mathf.Ceil(dimensions.x / meschScale) + 1;
        dimension.y = (int)Mathf.Ceil(dimensions.z / meschScale) + 1;

        node = new Node[(int)dimension.x][];
        for (int i = 0; i < (int)dimension.x; i++)
        {
            node[i] = new Node[(int)dimension.y];
            float x = i * meschScale;
            for (int j = 0; j < (int)dimension.y; j++)
            {
                float y = j * meschScale;
                float perlin = Mathf.PerlinNoise((float)i / perlinScale, (float)j / perlinScale);
                node[i][j] = new Node();
                node[i][j].position = new Vector3(x, perlin * dimensions.y, y);

                node[i][j].enabled = invertPerlin ?
                    perlin < perlinDensity :
                    perlin >= perlinDensity;
            }
        }
    }

    private void produceMeshes()
    {
        Vector2 n_dimension = new Vector2();
        n_dimension.x = (int)Mathf.Ceil(dimensions.x / meschScale);
        n_dimension.y = (int)Mathf.Ceil(dimensions.z / meschScale);
        Vector2 p_dimension = new Vector2();
        p_dimension.x = (int)Mathf.Ceil(dimensions.x / meschScale / dimensionCap);
        p_dimension.y = (int)Mathf.Ceil(dimensions.z / meschScale / dimensionCap);

        for (int i = 0; i < (int)p_dimension.x; i++)
        {
            for (int j = 0; j < (int)p_dimension.y; j++)
            {
                MarchingMesh m = new MarchingMesh();
                m.vertices = new List<Vector3>();
                m.triangles = new List<int>(); 

                int rowStart = i * dimensionCap;
                int rowCap = rowStart + dimensionCap;
                for (int x = rowStart; x < rowCap && x < (int)n_dimension.x; x++)
                {
                    int colStart = j * dimensionCap;
                    int colCap = colStart + dimensionCap;
                    for (int y = colStart; y < colCap && y < (int)n_dimension.y; y++)
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

                Mesh mesh = new Mesh();
                mesh.vertices = m.vertices.ToArray();
                mesh.triangles = m.triangles.ToArray();
                mesh.RecalculateNormals();
                patch[i][j].mesh = mesh;
            }
        }
    }
}
