using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileSelector : MonoBehaviour {
    #region Variables

    public static TileSelector instance;

    private TileActionMenu currentTile;

    public TileActionMenu CurrentTile { get => currentTile; set => currentTile = value; }

    #endregion

    #region Core

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Tile Selector instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
    }

    private void Update() {
        if (MatchManager.instance.MatchState != MatchState.InGame) {
            if (currentTile != null) currentTile.ToggleActive(false);
            currentTile = null;
            return;
        }

        OnLeftClick();
        OnRightClick();
    }

    private void OnLeftClick() {
        // Handle selecting units
        if (Input.GetKey(KeyCode.LeftShift)) {
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                if (currentTile != null) currentTile.ToggleActive(false);

                if (EventSystem.current.IsPointerOverGameObject()) return; // If cursor is over UI, return
                TileActionMenu newTile = GetTileUnderCursor();

                if (newTile == null) return;
                
                // Deselect all units
                for (int i = 0; i < UnitSelector.instance.SelectedUnits.Count; i++) {
                    UnitSelector.instance.SelectedUnits[i].Script.ToggleSelectedIndicator(false);
                }
                UnitSelector.instance.SelectedUnits = new List<UnitInfo>();
                
                // Get new units and select them
                List<int> selectedUnitUUIDs = UnitManager.instance.GetUnitsOfIdAtLocation(newTile.Tile.TileInfo.Location, MatchManager.instance.PlayerID);
                for (int i = 0; i < selectedUnitUUIDs.Count; i++) {
                    UnitSelector.instance.SelectedUnits.Add(UnitManager.instance.Units[selectedUnitUUIDs[i]]);
                    UnitSelector.instance.SelectedUnits[i].Script.ToggleSelectedIndicator(true);
                }
                return;
            }
        }
        if (UnitSelector.instance.SelectedUnits.Count > 0) {
            // Handle moving units
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                if (EventSystem.current.IsPointerOverGameObject()) return; // If cursor is over UI, return
                TileActionMenu newTile = GetTileUnderCursor();

                if (newTile == null) return;
                if (!PathfindingManager.instance.WalkableTileIds.Contains((int)newTile.Tile.TileInfo.TileId)) return;

                // Move units to new tile
                for (int i = 0; i < UnitSelector.instance.SelectedUnits.Count; i++) {
                    // TODO MULTIPLAYER
                    UnitSelector.instance.SelectedUnits[i].GameObject.GetComponent<PathfindingAgent>().PathfindToLocation(newTile.Tile.TileInfo.Location);
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.Mouse1)) {
                // Deselect all units
                for (int i = 0; i < UnitSelector.instance.SelectedUnits.Count; i++) {
                    UnitSelector.instance.SelectedUnits[i].Script.ToggleSelectedIndicator(false);
                }
                UnitSelector.instance.SelectedUnits = new List<UnitInfo>();
                return;
            }
            return;
        }

        // Handle selecting tiles
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            if (EventSystem.current.IsPointerOverGameObject()) return; // If cursor is over UI, return
            TileActionMenu newTile = GetTileUnderCursor();

            // If clicked on same tile, close menu.
            // If clicked on new tile, close old tile menu and open new tile menu.
            // If clicked on empty space, close old tile menu.
            if (newTile != null && currentTile != null && currentTile.gameObject == newTile.gameObject) {
                currentTile.ToggleActive(false);
            } else if (newTile != null) {
                if (currentTile) currentTile.ToggleActive(false);
                if (CheckTilePlayerID(newTile.Tile)) {
                    currentTile = newTile;
                    currentTile.ToggleActive(true);
                }
            } else {
                if (currentTile) currentTile.ToggleActive(false);
            }
        }
    }

    private void OnRightClick() {
        if (Input.GetKeyDown(KeyCode.Mouse1) && currentTile != null) {
            currentTile.ToggleActive(false);
            currentTile = null;
        }
    }

    #endregion

    #region Helper Methods

    private TileActionMenu GetTileUnderCursor() {
        // Get Layermask target of Raycast
        int layer_mask = LayerMask.GetMask("Tile");
        
        // Define "way overengineered"
        float maxDist = Mathf.Abs(Camera.main.transform.position.x) + Mathf.Abs(Camera.main.transform.position.y) + Mathf.Abs(Camera.main.transform.position.z);

        // Create Raycast and set it to mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        //do the raycast specifying the mask
        if (Physics.Raycast(ray, out hit, maxDist, layer_mask)) {
            return hit.transform.GetComponent<TileActionMenu>();
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

    #endregion
}
