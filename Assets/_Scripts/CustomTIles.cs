using UnityEngine;

public abstract class CustomTIles : ScriptableObject
{
    public float friction = 20f;
    public bool isWallJumpable = true;
    public AudioClip audio;
    public virtual void OnPlayerTouch(PlayerMovement player)
    {
        player.UpdateFriction(friction);
    }

    public virtual void OnWallContact(PlayerMovement player)
    {
        if (isWallJumpable)
        {
            player.EnableWallSlide();
        }

    }
}
[CreateAssetMenu(fileName ="Ground Tile", menuName = "Tiles/GroundTile")]
public class GroundTile : CustomTIles
{

}
[CreateAssetMenu(fileName = "Hazzard Tile", menuName ="Tiles/HazzardTile" )]
public class HazzardTile : CustomTIles
{
    public override void OnPlayerTouch(PlayerMovement player)
    {
        player.RespawnPlayer();
    }
}
