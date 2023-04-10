using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureActionButton : MonoBehaviour {
    [SerializeField] private StructureActionMenu structureActionMenu;
    [Space]
    [SerializeField] private int structureActionID;

    public void OnButtonDown() {
        USNL.PacketSend.StructureAction(structureActionID, structureActionMenu.TileActionMenu.Tile.TileInfo.Location, new int[] { } );
    }
}
