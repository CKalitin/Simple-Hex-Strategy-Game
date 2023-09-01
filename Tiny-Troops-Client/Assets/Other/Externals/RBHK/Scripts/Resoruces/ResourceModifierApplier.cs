using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceModifierApplier : MonoBehaviour {
    /*[Header("Config")]
    [Tooltip("Id of the player associated with this Resource Modifier Applier instance.")]
    [SerializeField] private int playerId;
    [Tooltip("If this resource modifier should apply only to one player.")]
    [SerializeField] private bool applyToSpecificPlayer = false;
    
    [Header("Resource Modidifer Applier")]
    [SerializeField] private ResourceModifier[] resourceModifiers;
    [SerializeField] private int effectRadius;

    private List<int> appliedResourceModifiers = new List<int>();

    [Header("Other")]
    [SerializeField] Tile tile;

    public int PlayerId { get => playerId; set => playerId = value; }
    public bool ApplyToSpecificPlayer { get => applyToSpecificPlayer; set => applyToSpecificPlayer = value; }

    public void ApplyResourceModifiers() {
        if (!TileManagement.instance.SpawningComplete) return;

        // Get all adjacent tiles 
        List<Vector2Int> locs = TileManagement.instance.GetAdjacentTilesInRadius(tile.TileInfo.Location, effectRadius);
        locs.Add(tile.TileInfo.Location); // Add tile of this script

        // Loop through adjacent tiles
        for (int i = 0; i < locs.Count; i++) {
            Tile targetTile = TileManagement.instance.GetTileAtLocation(locs[i]).Tile;

            for (int x = 0; x < resourceModifiers.Length; x++) {
                if (applyToSpecificPlayer) resourceModifiers[x].TargetPlayerID = playerId;
                appliedResourceModifiers.Add(targetTile.ResourceModifiers.Add(resourceModifiers[x]));
            }
            targetTile.UpdateResourceModifiers();
        }
    }

    public void RemoveResourceModifiers() {
        if (!TileManagement.instance.SpawningComplete) return;
        
        // Get all adjacent tiles 
        List<Vector2Int> locs = TileManagement.instance.GetAdjacentTilesInRadius(tile.TileInfo.Location, effectRadius);
        locs.Add(tile.TileInfo.Location); // Add tile of this script

        // Loop through adjacent tiles
        for (int i = 0; i < locs.Count; i++) {
            Tile targetTile = TileManagement.instance.GetTileAtLocation(locs[i]).Tile;

            for (int x = 0; x < appliedResourceModifiers.Count; x++) {
                targetTile.ResourceModifiers.Remove(appliedResourceModifiers[x]);
            }
            targetTile.UpdateResourceModifiers();
        }
    }

    private void OnDestroy() {
        RemoveResourceModifiers();
    }*/
}
