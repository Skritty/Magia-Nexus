using UnityEngine;

[CreateAssetMenu(menuName = "TileRef")]
public class WFCTileReferenceSO : ScriptableObject
{
    public (int,int,int) tileIndex;
    public WFCTileGroup group;
}
