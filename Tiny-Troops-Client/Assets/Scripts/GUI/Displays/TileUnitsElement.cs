using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TileUnitsElement : MonoBehaviour {
    [Header("Config")]
    [SerializeField] private int playerID;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI usernameText;
    [Space]
    [SerializeField] private TextMeshProUGUI unitCountText;
    [SerializeField] private TextMeshProUGUI tankCountText;
    [Space]
    [SerializeField] private TextMeshProUGUI healthText;
    [Space]
    [SerializeField] private TextMeshProUGUI unitAttackText;
    [SerializeField] private TextMeshProUGUI structureAttackText;

    [Header("References")]
    [SerializeField] private Tile tile;

    public int PlayerID { get => playerID; set => playerID = value; }

    private void Start() {
        tile = GameUtils.GetTileParent(transform);
    }

    private void Update() {
        SetText(UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location][playerID]);
    }

    public void SetText(UnitAttackManager.PlayerTileUnitInfo _tileInfo) {
        //UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location][MatchManager.instance.PlayerID];
        
        usernameText.text = PlayerInfoManager.instance.PlayerInfos[GameUtils.IdToIndex(playerID)].Username;
        
        unitCountText.text = _tileInfo.NumUnitsByType[0].ToString();
        //tankCountText.text = _tileInfo.NumUnitsByType[1].ToString();

        healthText.text = "Health " + _tileInfo.TotalCurrentHealth.ToString() + " / " + _tileInfo.TotalMaxHealth.ToString();

        unitAttackText.text = _tileInfo.UnitAttackDamage + " / " + _tileInfo.PotentialUnitAttackDamage;// + ", " + _tileInfo.PotentialUnitAttackDamage.ToString();
        structureAttackText.text = _tileInfo.StructureAttackDamage + " / " + _tileInfo.PotentialStructureAttackDamage;// + ", " + _tileInfo.PotentialStructureAttackDamage.ToString();
    }
}
