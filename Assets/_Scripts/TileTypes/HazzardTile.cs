using UnityEngine;

[CreateAssetMenu(fileName = "Hazzard Tile", menuName = "Tiles/HazzardTile")]
public class HazzardTile : CustomTiles
{
    public override void OnPlayerTouch(PlayerMovement player)
    {
        player.RespawnPlayer();
    }
}
