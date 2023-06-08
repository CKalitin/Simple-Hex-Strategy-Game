using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using USNL;

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
    float ogFooterY;

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
        for (int i = 0; i < USNL.ClientManager.instance.ServerInfo.ConnectedClientIds.Length; i++) {
            if (!UnitAttackManager.instance.TileAttackInfo.ContainsKey(tile.TileInfo.Location)) continue;
            UnitAttackManager.PlayerTileUnitInfo playerUnitInfo = UnitAttackManager.instance.TileAttackInfo[tile.TileInfo.Location][GameUtils.IdToIndex(USNL.ClientManager.instance.ServerInfo.ConnectedClientIds[i])];
            playerUnitCountsText[iters].transform.parent.gameObject.SetActive(true);
            playerUnitCountsText[iters].text = playerUnitInfo.NumUnitsByType[0].ToString() + " ";
            tileUnitsElements[iters].PlayerID = GameUtils.IdToIndex(USNL.ClientManager.instance.ServerInfo.ConnectedClientIds[i]);
            active = true;
            iters++;
        }

        for (int i = USNL.ClientManager.instance.ServerInfo.ConnectedClientIds.Length; i < playerUnitCountsText.Length; i++) {
            playerUnitCountsText[i].transform.parent.gameObject.SetActive(false);
        }

        togglableParent.SetActive(active);
        footer.localPosition = new Vector3(footer.localPosition.x, ogFooterY - (iters * footerSpacingPerElement) + (footerSpacingPerElement * playerUnitCountsText.Length), footer.localPosition.z);
    }
}
