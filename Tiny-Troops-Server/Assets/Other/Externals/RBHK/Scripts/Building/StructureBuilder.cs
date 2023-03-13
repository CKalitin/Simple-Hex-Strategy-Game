using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StructureBuilder : MonoBehaviour {
    [Header("Config")]
    [Tooltip("ID of the player associated with this Resource Management instance.")]
    [SerializeField] private int playerId;

    [Header("Building")]
    [SerializeField] private StructureBuildInfo currentStructureBuildInfo;
    [Space]
    [SerializeField] private bool buildingEnabled;
    [SerializeField] private bool destroyingEnabled;

    private bool buildingAllowedOnTile = false; // Determined in Builder(), if structure can be placed on tile

    public int PlayerId { get => playerId; set => playerId = value; }

    private GameObject displayStructure; // Transparent thingy that shows where it's gonna be built
    private StructureBuildInfo previousStructureBuildInfo; // This is for switching buildInfo and updating display structure

    public bool BuildingEnabled { get => buildingEnabled; set => buildingEnabled = value; }
    public StructureBuildInfo CurrentStructureBuildInfo { get => currentStructureBuildInfo; set => currentStructureBuildInfo = value; }

    void Update() {
        if (buildingEnabled) {
            Builder();
            DisplayStructure();
        } else if (destroyingEnabled) Destroyer();
    }

    #region Builder

    private void Builder() {
        Transform structureLocationsParent;
        Transform structureLoc;
        Transform tile;
        
        // If can't afford structure
        if (!CanAffordStructure()) { buildingAllowedOnTile = false; return; }
        // If there is no tile under the cursor
        if ((structureLocationsParent = GetTargetTile()) == null) { buildingAllowedOnTile = false; return; }
        // If there is no available structure location
        if ((structureLoc = GetClosestAvailableStructureLocation(structureLocationsParent, currentStructureBuildInfo.StructureSize)) == null) { buildingAllowedOnTile = false; return; }
        // If there is no Tile script on the parent
        if ((tile = structureLocationsParent.parent) == null) { buildingAllowedOnTile = false; return; }
        // If tile is not taken by another player
        if (!CheckTileStructuresPlayerIDs(tile.GetComponent<Tile>())) { buildingAllowedOnTile = false; return; }
        
        // If all checks are passed
        buildingAllowedOnTile = true;

        if (Input.GetMouseButtonDown(0)) {
            InstantiateStructure(structureLoc, tile);
        }
    }

    private void Destroyer() {
        Transform structureLocationsParent;
        Transform structureLoc;
        Transform tile;

        // Checks to exit function
        if ((structureLocationsParent = GetTargetTile()) == null) return; // If there is no tile under the cursor
        if ((structureLoc = GetClosestUnavailableStructureLocation(structureLocationsParent)) == null) return; // If there is no available structure location
        if ((tile = structureLocationsParent.parent) == null) return; // If there is no Tile script on the parent
        if (CheckTileStructuresPlayerIDs(tile.GetComponent<Tile>()) == false) return; // If structure on the tile belongs to another player

        if (Input.GetMouseButtonDown(1)) {
            structureLoc.GetComponent<StructureLocation>().AssignedStructure.GetComponent<Structure>().DestroyStructure();
        }
    }

    private void InstantiateStructure(Transform _structureLocation, Transform _parent) {
        GameObject newStructure = Instantiate(currentStructureBuildInfo.StructurePrefab, _structureLocation.position, Quaternion.identity, _parent);

        _structureLocation.GetComponent<StructureLocation>().AssignedStructure = newStructure;
        newStructure.GetComponent<Structure>().StructureLocation = _structureLocation.GetComponent<StructureLocation>();

        ApplyStructureCost();
    }

    private void DisplayStructure() {
        Transform structureLocationsParent;
        Transform structureLoc;
        Transform tile;

        // This is just here so Unity stops giving me warnings
        if (buildingAllowedOnTile) { }

        // Checks to exit function (Same as Builder())
        // If there is no tile under the cursor
        if ((structureLocationsParent = GetTargetTile()) == null) { DestroyDisplayStructure(); return; }
        // If there is no available structure location
        if ((structureLoc = GetClosestAvailableStructureLocation(structureLocationsParent, currentStructureBuildInfo.StructureSize)) == null) { DestroyDisplayStructure(); return; }
        // If there is no Tile script on the parent
        if ((tile = structureLocationsParent.parent) == null) { DestroyDisplayStructure(); return; }

        // If Structure Build Info has been changed or displayStructure is null
        if (previousStructureBuildInfo != currentStructureBuildInfo || !displayStructure) {
            DestroyDisplayStructure();
            displayStructure = Instantiate(currentStructureBuildInfo.StructureDisplayPrefab, structureLoc.position, Quaternion.identity);

            previousStructureBuildInfo = currentStructureBuildInfo;
        }

        displayStructure.transform.position = structureLoc.position;
    }

    private void DestroyDisplayStructure() {
        if (displayStructure)
            Destroy(displayStructure);
    }

    #endregion

    #region Builder Utils

    private Transform GetClosestAvailableStructureLocation(Transform _structureLocationsParent, int _structureSize) {
        List<Transform> structureLocs = GetStructureLocations(_structureLocationsParent, _structureSize);

        for (int i = 0; i < structureLocs.Count; i++) {
            if (structureLocs[i].GetComponent<StructureLocation>().AssignedStructure == null) {
                return structureLocs[i];
            }
        }
        return null;
    }

    private Transform GetClosestUnavailableStructureLocation(Transform _structureLocationsParent) {
        List<Transform> structureLocs = GetStructureLocations(_structureLocationsParent, -1);

        for (int i = 0; i < structureLocs.Count; i++) {
            if (structureLocs[i].GetComponent<StructureLocation>().AssignedStructure != null) {
                return structureLocs[i];
            }
        }
        return null;
    }

    // Returns all structure locations is structureSize <0
    private List<Transform> GetStructureLocations(Transform _structureLocationsParent, int _structureSize) {
       List<Transform> output = new List<Transform>();
           
        for (int i = 0; i < _structureLocationsParent.transform.childCount; i++) {
            // Return all structure locations if structureSize is lower than 0
            if (_structureSize < 0 && _structureLocationsParent.transform.GetChild(i).GetComponent<StructureLocation>()) {
                output.Add(_structureLocationsParent.transform.GetChild(i));
            } else if (_structureLocationsParent.transform.GetChild(i).GetComponent<StructureLocation>().StructureSize == _structureSize) {
                output.Add(_structureLocationsParent.transform.GetChild(i));
            }
        }

        // Sort by distance
        output = output.OrderBy((d) => (d.position - transform.position).sqrMagnitude).ToList();

        return output;
    }

    private Transform GetTargetTile() {
        // Get Layermask target of Raycast
        int layer_mask = LayerMask.GetMask("Tile");

        // Define "way overengineered"
        float maxDist = Mathf.Abs(Camera.main.transform.position.x) + Mathf.Abs(Camera.main.transform.position.y) + Mathf.Abs(Camera.main.transform.position.z);

        // Create Raycast and set it to mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        //do the raycast specifying the mask
        if (Physics.Raycast(ray, out hit, maxDist, layer_mask)) {
            return hit.transform;
        } else {
            return null;
        }
    }

    private bool CheckTileStructuresPlayerIDs(Tile _tile) {
        // Return false if there is another player's structure on the tile
        bool output = true;
        for (int i = 0; i < _tile.Structures.Count; i++) {
            if (_tile.Structures[i].PlayerId != playerId & _tile.Structures[i].PlayerId != -1) output = false;
        }
        return output;
    }

    public bool CanAffordStructure() {
        bool output = true;
        for (int i = 0; i < currentStructureBuildInfo.Cost.Length; i++) {
            if (currentStructureBuildInfo.Cost[i].Amount >= ResourceManager.instances[playerId].GetResource(currentStructureBuildInfo.Cost[i].Resource).Supply)
                output = false;
        }

        return output;
    }

    private void ApplyStructureCost() {
        for (int i = 0; i < currentStructureBuildInfo.Cost.Length; i++) {
            ResourceManager.instances[playerId].GetResource(currentStructureBuildInfo.Cost[i].Resource).Supply -= currentStructureBuildInfo.Cost[i].Amount;
        }
    }

    #endregion
}
