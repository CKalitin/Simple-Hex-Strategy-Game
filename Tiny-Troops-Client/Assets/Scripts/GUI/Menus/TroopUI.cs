using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TroopUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI unitsText;
    [SerializeField] private TextMeshProUGUI tanksText;
    [Space]
    [SerializeField] private TextMeshProUGUI unitDamageText;
    [SerializeField] private TextMeshProUGUI structureDamageText;
    [Space]
    [SerializeField] private TextMeshProUGUI totalHealthText;

    private void Update() {
        if (UnitSelector.instance.SelectedUnits.Count > 0) UpdateWithSelectedUnits();
        else UpdateWithAllUnits();
    }

    private void UpdateWithSelectedUnits() {
        unitsText.text = UnitSelector.instance.SelectedUnits.Count.ToString();
        tanksText.text = "0";
        
        unitDamageText.text = UnitAttackManager.instance.PlayerSelectedUnitInfo.TotalUnitAttackDamage.ToString();
        structureDamageText.text = UnitAttackManager.instance.PlayerSelectedUnitInfo.TotalStructureAttackDamage.ToString();

        totalHealthText.text = UnitAttackManager.instance.PlayerSelectedUnitInfo.TotalCurrentHealth.ToString() + "/" + UnitAttackManager.instance.PlayerSelectedUnitInfo.TotalMaxHealth.ToString();
    }

    private void UpdateWithAllUnits() {
        if (!UnitAttackManager.instance.PlayerUnitInfos.ContainsKey(MatchManager.instance.PlayerID)) {
            unitsText.text = "0";
            tanksText.text = "0";

            unitDamageText.text = "0";
            structureDamageText.text = "0";

            totalHealthText.text = "0/0";
            
            return;
        }
            
        unitsText.text = UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].NumUnitsByType[0].ToString();
        tanksText.text = "0";
        
        unitDamageText.text = UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].TotalUnitAttackDamage.ToString();
        structureDamageText.text = UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].TotalStructureAttackDamage.ToString();
        
        totalHealthText.text = UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].TotalCurrentHealth.ToString() + "/" + UnitAttackManager.instance.PlayerUnitInfos[MatchManager.instance.PlayerID].TotalMaxHealth.ToString();
    }
}
