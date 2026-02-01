using UnityEditor.Rendering;
using UnityEngine;

public enum TileType { Spikes, Collectible, Slippery, End}
public class TileScript : MonoBehaviour
{
    public TileType type;

}
