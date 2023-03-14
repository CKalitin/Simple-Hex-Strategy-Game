using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            currentTile = GetTileUnderCursor();
            if (currentTile != null) currentTile.ToggleActive(true);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1) && currentTile != null)
            currentTile.ToggleActive(false);
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

    public bool IsPointerOverUIElement() {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }

    #endregion
}
