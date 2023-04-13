using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TileUnitsDisplay : MonoBehaviour {
    [Header("UI")]
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject[] playerIdDisplays;
    [Space]
    [SerializeField] private TextMeshProUGUI unitCountText;
    [Space]
    [SerializeField] private TextMeshProUGUI totalHealthText;
    [Space]
    [SerializeField] private TextMeshProUGUI unitAttackText;
    [SerializeField] private TextMeshProUGUI structureAttackText;
    [Space]
    [SerializeField] private TextMeshProUGUI potentialUnitAttackText;
    [SerializeField] private TextMeshProUGUI potentialStructureAttackText;

    [Header("References")]
    [SerializeField] private Tile tile;

    private void Awake() {
        for (int i = 0; i < playerIdDisplays.Length; i++)
            playerIdDisplays[i].SetActive(false);
    }

    private void OnEnable() {
        UnitAttackManager.OnTileAttackInfoUpdated += OnTileAttackInfoUpdated;
    }

    private void OnDisable() {
        UnitAttackManager.OnTileAttackInfoUpdated -= OnTileAttackInfoUpdated;
    }

    private void OnTileAttackInfoUpdated() {
        UpdateUI();
    }

    private void UpdateUI() {
        if (!UnitAttackManager.instance.TileAttackInfo.ContainsKey(tile.TileInfo.Location) || !UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location].ContainsKey(MatchManager.instance.PlayerID)) {
            parent.SetActive(false);
            return;
        }

        UnitAttackManager.PlayerTileUnitInfo tileInfo = UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location][MatchManager.instance.PlayerID];

        if (tileInfo.Units.Count <= 0) {
            parent.SetActive(false);
            return;
        }
        
        parent.SetActive(true);

        playerIdDisplays[MatchManager.instance.PlayerID].SetActive(true);

        unitCountText.text = tileInfo.Units.Count.ToString();
        totalHealthText.text = tileInfo.TotalHealth.ToString();

        unitAttackText.text = tileInfo.PotentialUnitAttackDamage.ToString();
        structureAttackText.text = tileInfo.PotentialStructureAttackDamage.ToString();

        potentialUnitAttackText.text = tileInfo.UnitAttackDamage.ToString();
        potentialStructureAttackText.text = tileInfo.StructureAttackDamage.ToString();
    }
}
