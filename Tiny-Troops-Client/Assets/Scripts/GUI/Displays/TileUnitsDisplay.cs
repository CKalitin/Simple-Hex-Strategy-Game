using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TileUnitsDisplay : MonoBehaviour {
    [Header("UI")]
    [Tooltip("In order of playerID")]
    [SerializeField] private TileUnitsElement[] unitDisplays;
    [Space]
    [SerializeField] private TextMeshProUGUI playerUnitCountText;
    [SerializeField] private TextMeshProUGUI enemyUnitCountText;
    
    private Tile tile;

    private void Awake() {
        for (int i = 0; i < unitDisplays.Length; i++)
            unitDisplays[i].gameObject.SetActive(false);

        tile = GameUtils.GetTileParent(transform);
    }

    // This is late update so if an expanded Unit Display is activated, its text will always be updated before the first frame it is visible
    private void LateUpdate() {
        UpdateUI();
    }

    private void UpdateUI() {
        playerUnitCountText.text = "0";
        enemyUnitCountText.text = "0";

        if (!UnitAttackManager.instance.TileAttackInfo.ContainsKey(tile.TileInfo.Location)) {
            for (int i = 0; i < unitDisplays.Length; i++)
                unitDisplays[i].gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < unitDisplays.Length; i++) {
            int index = GameUtils.IdToIndex(MatchManager.instance.PlayerID);
            if (!UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location].ContainsKey(index)) {
                unitDisplays[i].gameObject.SetActive(false);
                continue;
            }

            UnitAttackManager.PlayerTileUnitInfo tileInfo = UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location][index];
            if (tileInfo.Units.Count <= 0) {
                unitDisplays[i].gameObject.SetActive(false);
                continue;
            }

            unitDisplays[i].gameObject.SetActive(true);
            unitDisplays[i].SetText(tileInfo);

            if (index == MatchManager.instance.PlayerID) {
                playerUnitCountText.text = tileInfo.Units.Count.ToString();
            } else {
                enemyUnitCountText.text = (int.Parse(playerUnitCountText.text) + tileInfo.Units.Count).ToString();
            }
        }
    }
}
