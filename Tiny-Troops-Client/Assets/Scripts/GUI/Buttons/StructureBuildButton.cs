using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureBuildButton : MonoBehaviour {
    [Tooltip("Leave null to use destroy tool.")]
    [SerializeField] private StructureBuildInfo structureBuildInfo;
    [Space]
    [SerializeField] private GameObject cantAffordParent;

    private void Update() {
        if (structureBuildInfo == null || cantAffordParent == null) return;

        if (ClientStructureBuilder.instance.CanAffordStructure(MatchManager.instance.PlayerID, structureBuildInfo)) {
            cantAffordParent.SetActive(false);
        } else {
            cantAffordParent.SetActive(true);
        }
    }

    public void OnButtonDown() {
        if (structureBuildInfo != null) {
            BuildManager.instance.DestroyDisplayStructure();
            BuildManager.instance.CurrentStructureBuildInfo = structureBuildInfo;
            BuildManager.instance.BuildingEnabled = true;
            BuildManager.instance.DestroyingEnabled = false;
        } else {
            BuildManager.instance.DestroyDisplayStructure();
            BuildManager.instance.BuildingEnabled = false;
            BuildManager.instance.DestroyingEnabled = true;
        }
        TileSelector.instance.Active = false;
    }
}
