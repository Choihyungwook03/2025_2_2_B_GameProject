using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelChunk : MonoBehaviour
{
    [Header("청크 설정")]
    public int chunkSize = 16;
    public int chunkHeight = 64;

    [Header("Perlin Noise 설정")]
    public float noiseScale = 0.1f;
    public int octaves = 3;
    public float persistence = 0.5f;
    public float lacunarity = 2.0f;

    [Header("지형 높이")]
    public int groundLevel = 32;
    public int heightVariation = 16;

    [Header("청크 배치 옵션")]
    public bool autoPositionByChunk = true;

    private BlockType[,,] blocks;
    private Mesh chunkMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    public Vector2Int chunkPosition;

    // Start is called before the first frame update
    void Start()
    {
        SetupMesh();

        if (autoPositionByChunk)
        {
            transform.position = new Vector3(chunkPosition.x * chunkSize, 0.0f, chunkPosition.y * chunkSize);
        }

        GenerateChunk();
        BuildMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetupMesh()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        Shader vertexColorShader = Shader.Find("Custom/VertexColor");
        if (vertexColorShader == null )
        {
            Debug.LogWarning("VertexColor 쉐이더를 찾을 수 없습니다.");
            vertexColorShader = Shader.Find("Unlit/Color");
        }

        meshRenderer.material = new Material(vertexColorShader);

        chunkMesh = new Mesh();
        chunkMesh.name = "VoxelChunk";

        chunkMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    }

    bool IsCaveAt(int x, int y, int z)
    {
        float caveScale = 0.05f;
        float caveThreshold = 0.55f;

        float cave1 = Mathf.PerlinNoise(x * caveScale, z * caveScale);
        float cave2 = Mathf.PerlinNoise(x * caveScale + 100, y * caveScale * 0.5f);
        float cave3 = Mathf.PerlinNoise(y * caveScale * 0.5f, z * caveScale + 200f);

        float caveValue = (cave1 + cave2 + cave3) / 3f;

        return caveValue > caveThreshold;
    }

    BlockType GetStoneWithOre(int x, int y, int z)
    {
        float oreNoise = Mathf.PerlinNoise(x * 0.1f + 500f, z * 0.1f + 500f);

        if (y < 10)
        {
            if (oreNoise > 0.95f)
                return BlockType.DiamondOre;
        }

        if (y < 20)
        {
            if (oreNoise > 0.92f)
                return BlockType.GoldOre;
        }

        if (y < 35)
        {
            if (oreNoise > 0.85f)
                return BlockType.IronOre;
        }

        if (oreNoise > 0.75f)
            return BlockType.CoalOre;

        return BlockType.Stone;
    }

    int GetTerrainHeight(int worldX, int WorldZ)
    {
        float amplitude = 1.0f;
        float frequency = 1.0f;
        float noiseHeight = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = worldX * noiseScale * frequency;
            float sampleZ = WorldZ * noiseScale * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ);
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= persistence;
        }
        int height = groundLevel + Mathf.RoundToInt(noiseHeight * heightVariation);
        return Mathf.Clamp(height, 1, chunkHeight - 1);
    }

    public void GenerateChunk()
    {
        blocks = new BlockType[chunkSize, chunkHeight, chunkSize];

        int waterLevel = 28;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int worldX = chunkPosition.x * chunkSize + x;
                int worldZ = chunkPosition.y * chunkSize + z;

                int height = GetTerrainHeight(worldX, worldZ);

                for (int y = 0; y < chunkHeight; y++)
                {
                    bool isCave = IsCaveAt(worldX, y, worldZ);

                    if (y == 0)
                    {
                        blocks[x, y, z] = BlockType.Bedrock;
                    }
                    else if(isCave && y > 5 && y < height - 1)
                    {
                        blocks[x, y, z] = BlockType.Air;
                    }
                    else if(y < height - 4)
                    {
                        blocks[x, y, z] = GetStoneWithOre(worldX, y, worldZ);
                    }
                    else if (y < height - 1)
                    {
                        blocks[x, y, z] = BlockType.Dirt;
                    }
                    else if (y == height - 1)
                    {
                        if (height > waterLevel + 1)
                        {
                            blocks[x, y, z] = BlockType.Grass;
                        }
                        else
                        {
                            blocks[x, y, z] = BlockType.Sand;
                        }
                    }
                    else if (y < waterLevel)
                    {
                        blocks[x, y, z] = BlockType.Water;
                    }
                    else
                    {
                        blocks[x, y, z] = BlockType.Air;
                    }
                }
            }
        }
    }

    void AddFace(int x, int y, int z, Vector3 direction, Color color, List<Vector3> vertices, List<int> traingles, List<Color> colors)
    {
        int vertCount = vertices.Count;
        Vector3 pos = new Vector3(x, y, z);

        if (direction == Vector3.up) // Y+
        {
            vertices.Add(pos + new Vector3(0, 1, 0));
            vertices.Add(pos + new Vector3(0, 1, 1));
            vertices.Add(pos + new Vector3(1, 1, 1));
            vertices.Add(pos + new Vector3(1, 1, 0));
        }
        else if (direction == Vector3.down) // Y-
        {
            vertices.Add(pos + new Vector3(0, 0, 0));
            vertices.Add(pos + new Vector3(1, 0, 0));
            vertices.Add(pos + new Vector3(1, 0, 1));
            vertices.Add(pos + new Vector3(0, 0, 1));
        }
        else if (direction == Vector3.forward) // Z+
        {
            vertices.Add(pos + new Vector3(0, 0, 1));
            vertices.Add(pos + new Vector3(1, 0, 1));
            vertices.Add(pos + new Vector3(1, 1, 1));
            vertices.Add(pos + new Vector3(0, 1, 1));
        }
        else if (direction == Vector3.back) // Z-
        {
            vertices.Add(pos + new Vector3(0, 0, 0));
            vertices.Add(pos + new Vector3(0, 1, 0));
            vertices.Add(pos + new Vector3(1, 1, 0));
            vertices.Add(pos + new Vector3(1, 0, 0));
        }
        else if (direction == Vector3.right) // X+
        {
            vertices.Add(pos + new Vector3(1, 0, 0));
            vertices.Add(pos + new Vector3(1, 1, 0));
            vertices.Add(pos + new Vector3(1, 1, 1));
            vertices.Add(pos + new Vector3(1, 0, 1));
        }
        else if (direction == Vector3.left) // X-
        {
            vertices.Add(pos + new Vector3(0, 0, 1));
            vertices.Add(pos + new Vector3(0, 1, 1));
            vertices.Add(pos + new Vector3(0, 1, 0));
            vertices.Add(pos + new Vector3(0, 0, 0));
        }

        // 삼각형 인덱스 (모든 면 공통)
        traingles.Add(vertCount + 0);
        traingles.Add(vertCount + 1);
        traingles.Add(vertCount + 2);
        traingles.Add(vertCount + 0);
        traingles.Add(vertCount + 2);
        traingles.Add(vertCount + 3);

        for (int i = 0; i < 4; i++)
        {
            colors.Add(color);
        }
    }

    bool IsTransparent(int x, int y, int z)
    {
        if (x < 0 || x >= chunkSize || y < 0 || y >= chunkHeight || z < 0 || z >= chunkSize)
            return true;

        return blocks[x, y, z] == BlockType.Air;
    } 

    void AddBlockFaces(int x, int y, int z, BlockType block, List<Vector3> vertices, List<int> triangles, List<Color> colors)
    {
        BlockData blockData = new BlockData(block);

        if (IsTransparent(x, y + 1, z))
        {
            AddFace(x, y, z, Vector3.up, blockData.blockColor, vertices, triangles, colors);
        }
        if (IsTransparent(x, y - 1, z))
        {
            AddFace(x, y, z, Vector3.down, blockData.blockColor, vertices, triangles, colors);
        }
        if (IsTransparent(x, y, z + 1))
        {
            AddFace(x, y, z, Vector3.forward, blockData.blockColor, vertices, triangles, colors);
        }
        if (IsTransparent(x, y, z - 1))
        {
            AddFace(x, y, z, Vector3.back, blockData.blockColor, vertices, triangles, colors);
        }
        if (IsTransparent(x + 1, y, z))
        {
            AddFace(x, y, z, Vector3.right, blockData.blockColor, vertices, triangles, colors);
        }
        if (IsTransparent(x - 1, y, z))
        {
            AddFace(x, y, z, Vector3.left, blockData.blockColor, vertices, triangles, colors);
        }
    }

    public void BuildMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    BlockType block = blocks[x, y, z];
                    if (block == BlockType.Air) continue;

                    AddBlockFaces(x, y, z, block, vertices, triangles, colors);
                }
            }
        }
        chunkMesh.Clear();
        chunkMesh.vertices = vertices.ToArray();
        chunkMesh.triangles = triangles.ToArray();
        chunkMesh.colors = colors.ToArray();
        chunkMesh.RecalculateNormals();

        meshFilter.mesh = chunkMesh;
        meshCollider.sharedMesh = chunkMesh;
    }
}
