using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionStructure : MonoBehaviour {
    [Tooltip("Structure that takes this ones place once it is done being constructed.")]
    [SerializeField] private StructureID constructedStructureID;
    [Space]
    [SerializeField] private Structure structure;
    [SerializeField] private Health health;
    [Space]
    [SerializeField] private float buildPercentage = 0.1f;

    private Tile tile;

    public StructureID ConstructedStructureID { get => constructedStructureID; set => constructedStructureID = value; }

    private void Awake() {
        GetTileParent();
    }

    private void GetTileParent() {
        Transform t = transform.parent;
        while (true) {
            if (t.GetComponent<Tile>()) {
                // If tile script found
                tile = t.GetComponent<Tile>();
                break;
            } else {
                // If we're at the top of the heirarchy, break out of the loop.
                if (t.parent == null) break;
                t = t.parent;
            }
        }
    }
    
    private void OnEnable() {
        USNL.CallbackEvents.OnStructureConstructionPacket += OnStructureConstructionPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnStructureConstructionPacket -= OnStructureConstructionPacket;
    }

    private void OnStructureConstructionPacket(object _packetObject) {
        USNL.StructureConstructionPacket packet = (USNL.StructureConstructionPacket)_packetObject;

        if (packet.Location != tile.TileInfo.Location) return;

        //buildPercentage = packet.Percentage;
        buildPercentage = health.CurrentHealth / health.MaxHealth;
        //health.CurrentHealth = Mathf.RoundToInt(health.MaxHealth * packet.Percentage);

        if (buildPercentage >= 1) { 
            //ClientStructureBuilder.instance.ReplaceConstructionStructure(tile.TileInfo.Location, (int)constructedStructureID, structure.PlayerID);
        }
    }
}
