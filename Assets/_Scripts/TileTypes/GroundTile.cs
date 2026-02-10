using UnityEngine;

[CreateAssetMenu(fileName = "Ground Tile", menuName = "Tiles/GroundTile")]
public class GroundTile : CustomTiles
{
    public override void OnPlayerTouch(PlayerMovement player)
    {
        base.OnPlayerTouch(player);
    }
    public override void OnWallContact(PlayerMovement player)
    {
        base.OnWallContact(player);
    }
}
