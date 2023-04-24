using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyStructureButton : MonoBehaviour {
    public void OnButtonDown() {
        ClientStructureBuilder.instance.DestroyStructureClient(TileSelector.instance.CurrentTile.Tile.TileInfo.Location);
        StructureUIManager.instance.DeactivateStructureUIs();
    }
}
