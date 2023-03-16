using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBuildButton : MonoBehaviour {
    [SerializeField] private Tile tile;
    [SerializeField] private TileActionMenu tileActionMenu;
    [Space]
    [SerializeField] private GameObject cantAffordDisplayParent;
    [Space]
    [SerializeField] private StructureBuildInfo sbi;
    [Space]
    [Tooltip("If this is a button that deletes the structure.")]
    [SerializeField] private bool deleteButton;

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
        else ClientStructureBuilder.instance.BuildStructureClient(tile.TileInfo.Location, sbi.StructurePrefab.GetComponent<Structure>().StructureID, sbi);
        tileActionMenu.ToggleActive(false);
    }
}
