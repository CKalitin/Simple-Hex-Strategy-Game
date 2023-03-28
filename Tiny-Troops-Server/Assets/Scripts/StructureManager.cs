using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureManager : MonoBehaviour {
    private void OnEnable() {
        USNL.CallbackEvents.OnStructureActionPacket += OnStructureActionPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnStructureActionPacket -= OnStructureActionPacket;
    }

    private void OnStructureActionPacket(object _packetObject) {
        USNL.StructureActionPacket packet = (USNL.StructureActionPacket)_packetObject;

        //TileManagement.instance.GetTileAtLocation(Vector2Int.RoundToInt(packet.TargetTileLocation)).
    }
}
