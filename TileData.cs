using UnityEngine;

public class TileData : MonoBehaviour
{
    [Header("Tile Identity")]
    public string tileType = "Default";

    [Header("Appearance")]
    public Color color = Color.HSVToRGB(0, 0, 0.7f);

    // Later you can extend with:
    // public Sprite sprite;
    // public AudioClip matchSound;
    // public AnimationClip deathAnim;
    // public int scoreValue;
}