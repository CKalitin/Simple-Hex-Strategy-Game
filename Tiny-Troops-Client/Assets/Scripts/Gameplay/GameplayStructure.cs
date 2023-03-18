using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    [SerializeField] private GameObject structureUI;

    private TileActionMenu tileActionMenu;

    public GameObject StructureUI { get => structureUI; set => structureUI = value; }
    public TileActionMenu TileActionMenu { get => tileActionMenu; set => tileActionMenu = value; }

    public void OnStructureUICloseButton() {
        tileActionMenu.ToggleActive(false);
    }
}
