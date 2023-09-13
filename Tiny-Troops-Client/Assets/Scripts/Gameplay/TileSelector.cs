using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileSelector : MonoBehaviour {
    #region Variables

    public static TileSelector instance;

    private TileSelectorCollider currentTile;

    private bool active = true;

    public TileSelectorCollider CurrentTile { get => currentTile; set => currentTile = value; }
    public bool Active { get => active; set => active = value; }

    #endregion

    #region Core

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Tile Selector instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    private void Update() {
        if (MatchManager.instance.MatchState != MatchState.InGame) {
            currentTile = null;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                if (active) OnShiftLeftClick();
            } else {
                if (active) OnLeftClick();
                else OnLeftClickInactive();
            }
        } else if (Input.GetKeyDown(KeyCode.Mouse1)) {
            if (Input.GetKey(KeyCode.LeftShift)) {
                if (active) OnShiftRightClick();
            } else {
                OnRightClick();
            }
        }

        HighlightVillager();
    }
    
    private void OnLeftClick() {
        if (!EventSystem.current.IsPointerOverGameObject() && UnitSelector.instance.SelectedUnits.Count > 0) MoveUnits();
        if (EventSystem.current.IsPointerOverGameObject()) BuildManager.instance.StopBuilding();
        else SelectTile();
    }

    // Tile Selector is inactive, Building Manager is active.
    private void OnLeftClickInactive() {
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (!BuildManager.instance.DestroyingEnabled) SelectTile();
            BuildManager.instance.StopBuilding();
        }
    }

    private void OnRightClick() {
        DeselectTile();
        DeselectAllUnits();
        BuildManager.instance.StopBuilding();
    }
    
    private void OnShiftLeftClick() {
        DeselectTile();
        SelectUnitsOnTile();
    }

    private void OnShiftRightClick() {
        DeselectTile();
        SelectUnitsOnTile(false);
    }

    #endregion

    #region On Click Functions

    private void SelectTile() {
        if (EventSystem.current.IsPointerOverGameObject()) return; // If cursor is over UI, return
        TileSelectorCollider newTile = GetTileUnderCursor();

        // If clicked on same tile, close menu.
        // If clicked on new tile, close old tile menu and open new tile menu.
        // If clicked on empty space, close old tile menu.
        if (newTile != null && currentTile != null && currentTile.gameObject == newTile.gameObject) {
            DeselectTile();
        } else if (newTile != null) {
            DeselectTile();
            if (CheckTilePlayerID(newTile.Tile)) {
                currentTile = newTile;

                // Set Structure UI variables
                if (newTile.Tile.Structures.Count > 0 && newTile.Tile.Structures[0].PlayerID >= 0) {
                    if (newTile.Tile.Structures[0].GetComponent<ClientUnitSpawner>())
                        StructureUIManager.instance.ClientUnitSpawner = newTile.Tile.Structures[0].GetComponent<ClientUnitSpawner>();
                    if (newTile.Tile.Structures[0].GetComponent<Health>())
                        StructureUIManager.instance.StructureHealth = newTile.Tile.Structures[0].GetComponent<Health>();
                    StructureUIManager.instance.Tile = newTile.Tile;

                    StructureUIManager.instance.ActivateStructureUI((int)newTile.Tile.Structures[0].StructureID);
                }
            }
        } else {
            DeselectTile();
        }
    }

    private void DeselectTile() {
        if (currentTile != null) {
            currentTile = null;
        }
        
        StructureUIManager.instance.DeactivateStructureUIs();
    }

    private void SelectUnitsOnTile(bool select = true) {
        if (EventSystem.current.IsPointerOverGameObject()) return; // If cursor is over UI, return
        TileSelectorCollider newTile = GetTileUnderCursor();

        if (newTile == null) return;
        
        List<int> selectedUnitUUIDs = UnitManager.instance.GetUnitsOfIdAtLocation(newTile.Tile.TileInfo.Location, MatchManager.instance.PlayerID);

        for (int i = 0; i < selectedUnitUUIDs.Count; i++) {
            if (select) SelectUnit(selectedUnitUUIDs[i]);
            else DeselectUnit(selectedUnitUUIDs[i]);
        }
    }

    private void DeselectAllUnits() {
        foreach (KeyValuePair<int, UnitInfo> unit in UnitSelector.instance.SelectedUnits) {
            UnitSelector.instance.SelectedUnits[unit.Key].Script.ToggleSelectedIndicator(false);
        }
        UnitSelector.instance.SelectedUnits = new Dictionary<int, UnitInfo>();
        
        if (SelectTroopsSlider.instance != null) SelectTroopsSlider.instance.SelectedUnits.Clear();
    }

    private void MoveUnits() {
        if (EventSystem.current.IsPointerOverGameObject()) return; // If cursor is over UI, return
        TileSelectorCollider newTile = GetTileUnderCursor();
        
        if (newTile == null) return;
        if (PathfindingManager.instance.UnwalkableTileIds.Contains(newTile.Tile.TileInfo.TileId)) return;

        // System<func>, beautiful
        USNL.PacketSend.UnitPathfind(UnitSelector.instance.SelectedUnits.Select(x => x.Value.Script.UnitUUID).ToArray(), newTile.Tile.TileInfo.Location);
    }

    #endregion

    #region Utils
    
    private void SelectUnit(int _unitUUID) {
        if (UnitSelector.instance.SelectedUnits.ContainsKey(_unitUUID)) return;
        UnitSelector.instance.SelectedUnits.Add(_unitUUID, UnitManager.instance.Units[_unitUUID]);
        UnitSelector.instance.SelectedUnits[_unitUUID].Script.ToggleSelectedIndicator(true);
    }

    public void DeselectUnit(int _unitUUID) {
        if (!UnitSelector.instance.SelectedUnits.ContainsKey(_unitUUID)) return;
        UnitSelector.instance.SelectedUnits[_unitUUID].Script.ToggleSelectedIndicator(false);
        UnitSelector.instance.SelectedUnits.Remove(_unitUUID);
        
        SelectTroopsSlider.instance.UnitDeselected(_unitUUID);
    }
    
    private TileSelectorCollider GetTileUnderCursor() {
        // Get Layermask target of Raycast
        int layer_mask = LayerMask.GetMask("Tile");
        
        // Define "way overengineered"
        float maxDist = Mathf.Abs(Camera.main.transform.position.x) + Mathf.Abs(Camera.main.transform.position.y) + Mathf.Abs(Camera.main.transform.position.z);

        // Create Raycast and set it to mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        //do the raycast specifying the mask
        if (Physics.Raycast(ray, out hit, maxDist, layer_mask)) {
            return hit.transform.GetComponent<TileSelectorCollider>();
        } else {
            return null;
        }
    }

    // If player is allowed to click on the tile
    private bool CheckTilePlayerID(Tile _tile) {
        if (_tile.Structures.Count <= 0) return true;
        if (_tile.Structures[0].GetComponent<Structure>().PlayerID == MatchManager.instance.PlayerID) return true; // MatchManager.instance.CurrentPlayerID genius copilot
        if (_tile.Structures[0].GetComponent<Structure>().PlayerID == -1) return true;
        return false;
    }

    private void HighlightVillager() {
        if (EventSystem.current.IsPointerOverGameObject()) return; // If cursor is over UI, return
        TileSelectorCollider tile = GetTileUnderCursor();
        if (tile == null) return;
        
        Villager[] villagers = FindObjectsOfType<Villager>();
        for (int i = 0; i < villagers.Length; i++) {
            if (villagers[i].PlayerID != MatchManager.instance.PlayerID) { 
                villagers[i].GetComponent<Villager>().ToggleSelectedIndicator(false);  
                continue; 
            }
            if (villagers[i].GetComponent<PathfindingAgent>().GetTargetLocation() != tile.Tile.TileInfo.Location) { 
                villagers[i].GetComponent<Villager>().ToggleSelectedIndicator(false); 
                continue; 
            }
            //if (villagers[i].GetComponent<PathfindingAgent>().CurrentLocation == tile.Tile.TileInfo.Location) continue; // Only show villagers moving to the tile, not on the tile
            villagers[i].GetComponent<Villager>().ToggleSelectedIndicator(true);
        }
    }

    #endregion
}
