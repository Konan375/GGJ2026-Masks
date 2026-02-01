using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Item Tile", menuName = "Tiles/ItemTile")]
public class ItemTile : Tile
{
    public string itemName;
    public int scroeValue;
    public bool consumeOnPickup = true;
}
