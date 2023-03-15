using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBuildButton : MonoBehaviour {
    [SerializeField] private Tile tile;
    [Space]
    [SerializeField] private StructureBuildInfo sbi;
    [Space]
    [Tooltip("If this is a button that deletes the structure.")]
    [SerializeField] private bool deleteButton;

    public void OnBuildButtonPressed() {
        if (deleteButton) ClientStructureBuilder.instance.DestroyStructureClient(tile.TileInfo.Location);
        else ClientStructureBuilder.instance.BuildStructureClient(tile.TileInfo.Location, sbi.StructurePrefab.GetComponent<Structure>().StructureID, sbi);
    }
}
