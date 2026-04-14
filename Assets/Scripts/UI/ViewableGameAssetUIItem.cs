using Skritty.Tools.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ViewableGameAssetUIItem : PooledObject, IUpdatable
{
    public ViewableGameAsset asset;
    public TextMeshProUGUI assetName, assetInfo;
    public Image image;
    public void UpdateUI()
    {
        if(assetName != null) assetName.text = asset.name;
        if (assetInfo != null) assetInfo.text = asset.info;
        if (image != null) image.sprite = asset.image;
    }
}
