using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    Air,
    Grass,
    Dirt,
    Stone,
    Bedrock,
    Wood,
    Leaf,
    Water,
    Sand,
    CoalOre,
    IronOre,
    GoldOre,
    DiamondOre,
}

[System.Serializable]

public class BlockData
{
    public BlockType blockType;
    public Color blockColor;
    public bool isSolid;

    public BlockData(BlockType type)
    {
        blockType = type;
        isSolid = type != BlockType.Air;

        switch (type)
        {
            case BlockType.Grass:
                blockColor = new Color(0.2f, 0.8f, 0.2f);
                break;
            case BlockType.Dirt:
                blockColor = new Color(0.6f, 0.4f, 0.2f);
                break;
            case BlockType.Stone:
                blockColor = new Color(0.5f, 0.5f, 0.5f);
                break;
            case BlockType.Bedrock:
                blockColor = new Color(0.2f, 0.2f, 0.2f);
                break;
            case BlockType.Wood:
                blockColor = new Color(0.6f, 0.3f, 0.1f);
                break;
            case BlockType.Leaf:
                blockColor = new Color(0.1f, 0.6f, 0.1f);
                break;
            case BlockType.Water:
                blockColor = new Color(0.2f, 0.4f, 0.9f);
                isSolid = false;
                break;
            case BlockType.Sand:
                blockColor = new Color(0.9f, 0.85f, 0.6f);
                break;
            case BlockType.CoalOre:
                blockColor = new Color(0.3f, 0.3f, 0.3f);
                break;
            case BlockType.IronOre:
                blockColor = new Color(0.3f, 0.3f, 0.3f);
                break;
            case BlockType.GoldOre:
                blockColor = new Color(0.3f, 0.3f, 0.3f);
                break;
            case BlockType.DiamondOre:
                blockColor = new Color(0.3f, 0.3f, 0.3f);
                break;

            default:
                blockColor = Color.clear;
                isSolid = false;
                break;
        }
    }
}
