using UnityEngine;
using UnityEngine.Tilemaps;
public enum Tiles { Spikes, Collectible, Slippery, End }
[CreateAssetMenu(fileName = "New Item Tile", menuName = "Tiles/Types")]

public class TileTypes : Tile
{
    public Tiles types;
}
