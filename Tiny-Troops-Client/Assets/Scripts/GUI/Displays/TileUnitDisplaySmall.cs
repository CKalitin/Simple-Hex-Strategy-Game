using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using USNL;
using System;

public class TileUnitDisplaySmall : MonoBehaviour {
    [Tooltip("In order of playerID, mostly.")]
    [SerializeField] private TextMeshProUGUI[] playerUnitCountsText;
    [Tooltip("In same order as playerUnitCountsText")]
    [SerializeField] private TileUnitsElement[] tileUnitsElements;
    [Space]
    [SerializeField] private GameObject togglableParent;
    [Space]
    [SerializeField] private Transform footer;
    [SerializeField] private float footerSpacingPerElement = 0.5f;
    private float ogFooterY;

    [Header("References")]
    [SerializeField] private Tile tile;

    private void Start() {
        ogFooterY = footer.localPosition.y;
        tile = GameUtils.GetTileParent(transform);
        togglableParent.SetActive(false);
    }

    private void Update() {
        SetText();
    }

    public void SetText() {
        if (!ClientManager.instance.IsConnected) return;
        int iters = 0;
        bool active = false;
        int lastClientID = -1;

        playerUnitCountsText[0].text = "0";

        // If client does not have troops on this tile, increase iters to account for the enemy using a troop count display
        if (UnitAttackManager.instance.TileAttackInfo.ContainsKey(tile.TileInfo.Location) && !UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location].ContainsKey(MatchManager.instance.PlayerID)) iters++;
        
        for (int i = 0; i < USNL.ClientManager.instance.ServerInfo.ConnectedClientIds.Length; i++) {
            if (!UnitAttackManager.instance.TileAttackInfo.ContainsKey(tile.TileInfo.Location)) continue;
            
            int clientID = USNL.ClientManager.instance.ServerInfo.ConnectedClientIds[i];
            if (!UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location].ContainsKey(clientID)) continue;
            
            int index = iters;
            if (index == 0) index = GameUtils.IdToIndex(iters);
            if (clientID == MatchManager.instance.PlayerID) index = 0;
            
                
            UnitAttackManager.PlayerTileUnitInfo playerUnitInfo = UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location][clientID];
            playerUnitCountsText[index].transform.parent.gameObject.SetActive(true);
            playerUnitCountsText[index].text = playerUnitInfo.NumUnitsByType[0].ToString();

            tileUnitsElements[index].PlayerID = clientID;
            if (playerUnitCountsText[index].transform.parent.GetComponent<PlayerColourSetter>()) {
                playerUnitCountsText[index].transform.parent.GetComponent<PlayerColourSetter>().ClientID = clientID;
                playerUnitCountsText[index].transform.parent.GetComponent<PlayerColourSetter>().UpdateColor();
                lastClientID = clientID;
            }
            active = true;
            iters++;
        }

        for (int i = iters; i < playerUnitCountsText.Length; i++) {
            playerUnitCountsText[i].transform.parent.gameObject.SetActive(false);
        }
        playerUnitCountsText[0].transform.parent.gameObject.SetActive(true);

        if (lastClientID != MatchManager.instance.PlayerID && lastClientID != -1) {
            footer.GetComponent<PlayerColourSetter>().ClientID = lastClientID;
            footer.GetComponent<PlayerColourSetter>().UpdateColor();
        } else {
            footer.GetComponent<PlayerColourSetter>().SetToDefaultColor();
        }

        togglableParent.SetActive(active);
        footer.localPosition = new Vector3(footer.localPosition.x, ogFooterY - (iters * footerSpacingPerElement) + (footerSpacingPerElement * playerUnitCountsText.Length), footer.localPosition.z);
    }
}
