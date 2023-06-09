using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class TileUnitsElement : MonoBehaviour {
    [Header("Config")]
    [SerializeField] private int playerID;

    int index = 0;

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

        MatchManager.instance.SetPlayerID();
        index = playerID;
        if (playerID == 0) index = GameUtils.IdToIndex(playerID);
        if (playerID == MatchManager.instance.PlayerID) index = 0;
    }

    private void Update() {
        if (UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location].ContainsKey(index))
            SetText(UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location][index]);
        else SetTextToDefault();
    }

    public void SetText(UnitAttackManager.PlayerTileUnitInfo _tileInfo) {
        //UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location][MatchManager.instance.PlayerID];
        
        usernameText.text = PlayerInfoManager.instance.PlayerInfos[index].Username;
        
        unitCountText.text = _tileInfo.NumUnitsByType[0].ToString();
        //tankCountText.text = _tileInfo.NumUnitsByType[1].ToString();

        healthText.text = "Health " + _tileInfo.TotalCurrentHealth.ToString() + " / " + _tileInfo.TotalMaxHealth.ToString();

        unitAttackText.text = _tileInfo.UnitAttackDamage + " / " + _tileInfo.PotentialUnitAttackDamage;
        structureAttackText.text = _tileInfo.StructureAttackDamage + " / " + _tileInfo.PotentialStructureAttackDamage;
    }

    public void SetTextToDefault() {
        usernameText.text = "";

        unitCountText.text = "0";
        if (tankCountText) tankCountText.text = "0";

        healthText.text = "Health 0 / 0";

        unitAttackText.text = "0 / 0";
        structureAttackText.text = "0 / 0";
    }
}
