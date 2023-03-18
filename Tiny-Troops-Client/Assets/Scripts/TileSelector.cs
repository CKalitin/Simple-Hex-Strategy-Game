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
            Debug.Log($"Match Manager instance already exists on ({gameObject}), destroying this.");
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
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            TileActionMenu newTile = GetTileUnderCursor();
            // If cursor is over UI, return.
            if (EventSystem.current.IsPointerOverGameObject()) return;

            // If clicked on same tile, close menu.
            // If clicked on new tile, close old tile menu and open new tile menu.
            // If clicked on empty space, close old tile menu.
            if (newTile != null && currentTile != null && currentTile.gameObject == newTile.gameObject) {
                currentTile.ToggleActive(false);
            } else if (newTile != null) {
                if (currentTile) currentTile.ToggleActive(false);
                currentTile = newTile;
                currentTile.ToggleActive(true);
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

    #endregion
}
