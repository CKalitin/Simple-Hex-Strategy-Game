using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBuildButton : MonoBehaviour {
    [SerializeField] private Tile tile;
    [SerializeField] private TileActionMenu tileActionMenu;
    [SerializeField] private StructureActionMenu structureActionMenu;
    [Space]
    [SerializeField] private GameObject cantAffordDisplayParent;
    [Space]
    [SerializeField] private StructureBuildInfo sbi;
    [Space]
    [Tooltip("If this is a button that deletes the structure.")]
    [SerializeField] private bool deleteButton;

    private void Start() {
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

    private void Update() {
        if (deleteButton || sbi == null || cantAffordDisplayParent == null) return;

        if (ClientStructureBuilder.instance.CanAffordStructure(USNL.ClientManager.instance.ClientId, sbi)) {
            cantAffordDisplayParent.SetActive(false);
        } else {
            cantAffordDisplayParent.SetActive(true);
        }
    }

    public void OnBuildButtonPressed() {
        if (deleteButton) ClientStructureBuilder.instance.DestroyStructureClient(tile.TileInfo.Location);
        else {
            if (tile.Structures.Count > 0) return;
            ClientStructureBuilder.instance.BuildStructureClient(tile.TileInfo.Location, sbi.StructurePrefab.GetComponent<Structure>().StructureID, sbi);
        }
        
        if (tileActionMenu) tileActionMenu.ToggleActive(false);
        if (structureActionMenu && structureActionMenu.TileActionMenu) structureActionMenu.TileActionMenu.ToggleActive(false);
    }
}
