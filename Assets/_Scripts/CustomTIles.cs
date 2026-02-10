using UnityEngine;

public abstract class CustomTiles : ScriptableObject
{
    public float friction = 20f;
    public float wallSlidingSpeed = 2f;
    public bool isWallJumpable = true;
    public AudioClip contactAudio;
    public virtual void OnPlayerTouch(PlayerMovement player)
    {
        player.UpdateFriction(friction);
    }

    public virtual void OnWallContact(PlayerMovement player)
    {
        if (isWallJumpable)
        {
            player.EnableWallSlide();
            player.UpdateFriction(friction);
        }

    }
}

