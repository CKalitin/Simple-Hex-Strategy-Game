using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureProductionDisplay : MonoBehaviour {
    [SerializeField] private GameObject togglableParent;

    private Tile tile;
    private GameplayStructure gameplayStructure;

    private void Start() {
        tile = GameUtils.GetTileParent(transform);
        togglableParent.SetActive(false);
    }

    private void Update() {
        if (gameplayStructure == null) {
            if (tile.Structures.Count > 0 && tile.Structures[0].GetComponent<GameplayStructure>()) gameplayStructure = tile.Structures[0].GetComponent<GameplayStructure>();
            togglableParent.SetActive(false);
            return;
        }

        togglableParent.SetActive(!gameplayStructure.ProductionEnabled);
    }
}
