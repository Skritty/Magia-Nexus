using UnityEngine;

[CreateAssetMenu(menuName = "TileRef")]
public class WFCTileReferenceSO : ScriptableObject
{
    public int xIndex, yIndex, zIndex;
    public WFCTileGroup group;
}
