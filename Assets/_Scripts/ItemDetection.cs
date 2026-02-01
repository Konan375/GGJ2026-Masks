using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemDetection : MonoBehaviour
{

    public Tilemap itemTilemap;

    // Update is called once per frame
    void Update()
    {
        Vector3Int gridPos = itemTilemap.WorldToCell(transform.position);
        TileBase tile = itemTilemap.GetTile(gridPos);

        if (tile is ItemTile item){
            Debug.Log($"Picked UP: {item.itemName}");
            if (item.consumeOnPickup)
            {
                itemTilemap.SetTile(gridPos, null);
            }


        }
    }
}
