using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureActionMenu : MonoBehaviour {
    private TileActionMenu tileActionMenu;

    public TileActionMenu TileActionMenu { get => tileActionMenu; set => tileActionMenu = value; }

    public void OnCloseButton() {
        tileActionMenu.ToggleActive(false);
    }
}
