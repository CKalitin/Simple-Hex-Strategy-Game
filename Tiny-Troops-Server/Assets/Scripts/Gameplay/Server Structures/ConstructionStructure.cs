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

    public Vector2Int Location { get => tile.TileInfo.Location; }
    public Structure Structure { get => structure; set => structure = value; }
    public int PlayerID { get => structure.PlayerID; }

    private void Awake() {
        GetTileParent();
        VillagerManager.instance.AddConstructionStructure(Location, this);
    }

    private void Start() {
        VillagerManager.instance.SetVillagersTargetLocation();
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

    private void OnDestroy() {
        Debug.Log("OnDestroy");
        VillagerManager.instance.RemoveConstructionStructure(this);
        VillagerManager.instance.SetVillagersTargetLocation();
    }

    public void ChangeBuildPercentage(float _change) {
        buildPercentage += _change;
        health.CurrentHealth = health.MaxHealth / buildPercentage;

        USNL.PacketSend.StructureConstruction(Location, buildPercentage);
        
        if (buildPercentage >= 1f) {
            Debug.Log("Structure Done");
            ServerStructureBuilder.instance.ReplaceConstructionStructure(Location, (int)constructedStructureID, structure.PlayerID);
            Debug.Log("Structure Built");
        }
    }

    public void SetBuildPercentage(float _buildPercentage) {
        buildPercentage = _buildPercentage;
        health.CurrentHealth = health.MaxHealth / buildPercentage;

        USNL.PacketSend.StructureConstruction(Location, buildPercentage);

        if (buildPercentage >= 1f) {
            Debug.Log("Structure Done");
            ServerStructureBuilder.instance.ReplaceConstructionStructure(Location, (int)constructedStructureID, structure.PlayerID);
            Debug.Log("Structure Built");
        }
    }
}
