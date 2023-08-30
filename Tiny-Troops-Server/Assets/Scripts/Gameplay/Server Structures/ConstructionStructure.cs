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

    private float previousHealth = -1f; // This is -1 so on the first Update() it gets updated
    
    public Vector2Int Location { get => tile.TileInfo.Location; }
    public Structure Structure { get => structure; set => structure = value; }
    public int PlayerID { get => structure.PlayerID; }
    public StructureID ConstructedStructureID { get => constructedStructureID; set => constructedStructureID = value; }

    public Vector2Int TileLocation { get => tile.TileInfo.Location;  }
    
    private void Awake() {
        tile = GameUtils.GetTileParent(transform);
    }

    private void Start() {
        VillagerManager.instance.AddConstructionStructure(Location, this);
        VillagerManager.instance.UpdateVillagersConstruction();
    }

    private void Update() {
        if (previousHealth != health.CurrentHealth) {
            previousHealth = health.CurrentHealth;

            USNL.PacketSend.StructureHealth(TileLocation, health.CurrentHealth, health.MaxHealth);
            if (health.CurrentHealth <= 0) {
                GetComponent<Structure>().DestroyStructure();
                VillagerManager.instance.RemoveDestroyStructure(TileLocation, GetComponent<GameplayStructure>());
                StartCoroutine(VillagerManager.instance.UpdateVillagersConstructionDelayed(0.1f));
            }
        }

        StartCoroutine(VillagerManager.instance.UpdateVillagersConstructionDelayed(0.1f));
    }

    private void OnDestroy() {
        VillagerManager.instance.RemoveConstructionStructure(this);
        VillagerManager.instance.UpdateVillagersConstruction();
    }

    public void ChangeBuildPercentage(float _change) {
        health.ChangeHealth(health.MaxHealth * _change);
        buildPercentage = health.CurrentHealth / health.MaxHealth;

        if (buildPercentage >= 1f) {
            // This is here so the client construction structure is refunded properly
            USNL.PacketSend.StructureConstruction(TileLocation, 1f);
            USNL.PacketSend.StructureHealth(TileLocation, health.CurrentHealth, health.MaxHealth);

            structure.ApplyFullRefunds = true;
            ServerStructureBuilder.instance.ReplaceStructure(Location, (int)constructedStructureID, structure.PlayerID);
        }
    }

    public void SetBuildPercentage(float _buildPercentage) {
        buildPercentage = _buildPercentage;
        health.SetHealth(health.MaxHealth / _buildPercentage);

        if (buildPercentage >= 1f) {
            // This is here so the client construction structure is refunded properly
            USNL.PacketSend.StructureConstruction(TileLocation, 1f);
            USNL.PacketSend.StructureHealth(TileLocation, health.CurrentHealth, health.MaxHealth);

            structure.ApplyFullRefunds = true;
            ServerStructureBuilder.instance.ReplaceStructure(Location, (int)constructedStructureID, structure.PlayerID);
        }
    }
}
