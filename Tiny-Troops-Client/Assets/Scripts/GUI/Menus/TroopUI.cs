using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class TroopUI : MonoBehaviour {
    [Header("Parents")]
    [SerializeField] private GameObject titleParent;
    [SerializeField] private GameObject selectTroopsParent;

    [Header("Troop Info")]
    [SerializeField] private TextMeshProUGUI unitsText;
    [SerializeField] private TextMeshProUGUI tanksText;
    [Space]
    [SerializeField] private TextMeshProUGUI unitDamageText;
    [SerializeField] private TextMeshProUGUI structureDamageText;
    [Space]
    [SerializeField] private TextMeshProUGUI totalHealthText;

    [Header("Other")]
    [SerializeField] private int[] troopUnitIDs;
    [SerializeField] private int[] tankUnitIDs;

    private void Update() {
        if (UnitSelector.instance.SelectedUnits.Count > 0) UpdateWithSelectedUnits();
        else UpdateWithAllUnits();
    }

    private void UpdateWithSelectedUnits() {
        titleParent.SetActive(false);
        selectTroopsParent.SetActive(true);

        int unitsCount = 0;
        int tanksCount = 0;
        for (int i = 0; i < UnitSelector.instance.SelectedUnitsById.Count; i++) if (troopUnitIDs.Contains(i)) unitsCount += UnitSelector.instance.SelectedUnitsById[i].Count;
        for (int i = 0; i < UnitSelector.instance.SelectedUnitsById.Count; i++) if (tankUnitIDs.Contains(i)) tanksCount += UnitSelector.instance.SelectedUnitsById[i].Count;
        unitsText.text = unitsCount.ToString();
        tanksText.text = tanksCount.ToString();
        
        unitDamageText.text = UnitAttackManager.instance.PlayerSelectedUnitInfo.TotalUnitAttackDamage.ToString();
        structureDamageText.text = UnitAttackManager.instance.PlayerSelectedUnitInfo.TotalStructureAttackDamage.ToString();

        totalHealthText.text = UnitAttackManager.instance.PlayerSelectedUnitInfo.TotalCurrentHealth.ToString() + "/" + UnitAttackManager.instance.PlayerSelectedUnitInfo.TotalMaxHealth.ToString();
    }

    private void UpdateWithAllUnits() {
        titleParent.SetActive(true);
        selectTroopsParent.SetActive(false);
        
        if (!UnitAttackManager.instance.PlayerUnitInfos.ContainsKey(MatchManager.instance.PlayerID)) {
            unitsText.text = "0";
            tanksText.text = "0";

            unitDamageText.text = "0";
            structureDamageText.text = "0";

            totalHealthText.text = "0/0";
            
            return;
        }

        int unitsCount = 0;
        int tanksCount = 0;
        for (int i = 0; i < UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].NumUnitsByType.Count; i++) if (troopUnitIDs.Contains(i)) unitsCount += UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].NumUnitsByType[i];
        for (int i = 0; i < UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].NumUnitsByType.Count; i++) if (tankUnitIDs.Contains(i)) tanksCount += UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].NumUnitsByType[i];

        unitsText.text = unitsCount.ToString();
        tanksText.text = tanksCount.ToString();
        
        unitDamageText.text = UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].TotalUnitAttackDamage.ToString();
        structureDamageText.text = UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].TotalStructureAttackDamage.ToString();
        
        totalHealthText.text = UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].TotalCurrentHealth.ToString() + "/" + UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].TotalMaxHealth.ToString();
    }
}
