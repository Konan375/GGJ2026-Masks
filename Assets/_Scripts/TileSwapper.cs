using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.Tilemaps;
public class TileSwapper : MonoBehaviour
{
    public Tilemap targetTilemap;
    public TileBase oldTile;
    public CustomDataTile newTile;

    [ContextMenu("SwapTiles")]
    public void SwapAllTiles()
    {
        BoundsInt bounds = targetTilemap.cellBounds;
        TileBase[] allTiles = targetTilemap.GetTilesBlock(bounds);
        int swapCount = 0;

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * bounds.size.x];

                if (tile == oldTile)
                {
                    Vector3Int pos = new Vector3Int(x + bounds.xMin, y + bounds.yMin, bounds.zMin);
                    targetTilemap.SetTile(pos, newTile);
                    swapCount++;

                }
            }
        }
        Debug.Log($"Successfully swapped {swapCount} tiles");
    }

}
