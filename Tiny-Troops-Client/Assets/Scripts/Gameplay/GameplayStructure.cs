using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    [SerializeField] private GameObject structureUI;

    private TileActionMenu tileActionMenu;

    public GameObject StructureUI { get => structureUI; set => structureUI = value; }

    public void OnStructureUICloseButton() {
        tileActionMenu.ToggleActive(false);
    }

    public void SetTileActionMenu(TileActionMenu _tam) {
        tileActionMenu = _tam;
        structureUI.GetComponent<StructureActionMenu>().TileActionMenu = _tam;
    }
}
