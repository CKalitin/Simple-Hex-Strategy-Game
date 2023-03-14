using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBuildButton : MonoBehaviour {
    [SerializeField] private Tile tile;
    [Space]
    [SerializeField] private StructureBuildInfo sbi;

    public void OnBuildButtonPressed() {
        ClientStructureBuilder.instance.BuildStructureClient(tile.TileInfo.Location, sbi.StructurePrefab.GetComponent<Structure>().StructureID, sbi);
    }
}
