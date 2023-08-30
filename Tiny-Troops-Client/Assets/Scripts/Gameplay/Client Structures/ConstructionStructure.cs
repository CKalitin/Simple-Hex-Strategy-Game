using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using USNL;

public class ConstructionStructure : MonoBehaviour {
    [Tooltip("Structure that takes this ones place once it is done being constructed.")]
    [SerializeField] private StructureID constructedStructureID;
    [Space]
    [SerializeField] private Structure structure;
    [SerializeField] private Health health;

    private Tile tile;

    public StructureID ConstructedStructureID { get => constructedStructureID; set => constructedStructureID = value; }

    private void Awake() {
        tile = GameUtils.GetTileParent(transform);
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnStructureConstructionPacket += OnStructureConstructionPacket;
        USNL.CallbackEvents.OnStructureHealthPacket += OnStructureHealthPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnStructureConstructionPacket -= OnStructureConstructionPacket;
        USNL.CallbackEvents.OnStructureHealthPacket -= OnStructureHealthPacket;
    }

    private void OnStructureConstructionPacket(object _packetObject) {
        USNL.StructureConstructionPacket packet = (USNL.StructureConstructionPacket)_packetObject;

        if (packet.Location != tile.TileInfo.Location) return;

        if (packet.Percentage >= 1f) structure.DontApplyRefunds = true;
    }

    private void OnStructureHealthPacket(object _object) {
        StructureHealthPacket packet = (StructureHealthPacket)_object;
        if (packet.Health >= packet.MaxHealth) structure.DontApplyRefunds = true;
    }
}
