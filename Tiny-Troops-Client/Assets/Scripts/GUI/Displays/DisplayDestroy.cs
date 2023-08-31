using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayDestroy : MonoBehaviour {
    #region Variables

    [Header("Display")]
    [SerializeField] private ResourceDisplay productionDisplay;
    [SerializeField] private ResourceDisplay[] resourceDisplays;

    [Header("Other")]
    [SerializeField] private Sprite[] resourceIcons;

    private Tile tile;
    private Vector2Int previousLocation;

    public Tile Tile { get => tile; set => tile = value; }
    
    [Serializable]
    private struct ResourceDisplay {
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI resourceTypeText;

        public TextMeshProUGUI AmountText { get => amountText; set => amountText = value; }
        public Image Icon { get => icon; set => icon = value; }
        public TextMeshProUGUI ResourceTypeText { get => resourceTypeText; set => resourceTypeText = value; }
    }

    #endregion

    #region Core

    private void Awake() {
        ClearDisplays();
    }

    private void Update() {
        Structure structure = null;
        NaturalStructure naturalStructure = null;
        
        // Tile is set in BuildManager.cs
        if (tile == null) { ClearDisplays(); return; }
        if (tile.Structures.Count <= 0) { ClearDisplays(); return; }
        if ((structure = tile.Structures[0]) == null) { ClearDisplays(); return; }

        if (previousLocation != tile.TileInfo.Location) ClearDisplays();

        if (structure.ResourceEntries.Length > 0 && Mathf.RoundToInt(structure.ResourceEntries[0].Change) != 0) {
            productionDisplay.AmountText.text = "-" + Mathf.RoundToInt(structure.ResourceEntries[0].Change).ToString();
            productionDisplay.AmountText.color = GameController.instance.NegativeColor;
            productionDisplay.Icon.sprite = resourceIcons[(int)structure.ResourceEntries[0].ResourceId];
            productionDisplay.Icon.color = new Color(255, 255, 255, 255);
            if (structure.ResourceEntries[0].ChangeOnTick) productionDisplay.ResourceTypeText.text = " / Tick";
        }

        if ((naturalStructure = structure.GetComponent<NaturalStructure>()) != null) {
            productionDisplay.AmountText.text = "+" + naturalStructure.Resources[0].Amount.ToString();
            productionDisplay.AmountText.color = GameController.instance.PositiveColor;
            productionDisplay.Icon.sprite = resourceIcons[(int)naturalStructure.Resources[0].Resource];
            productionDisplay.Icon.color = new Color(255, 255, 255, 255);
        }

        Dictionary<GameResource, float> resources = new Dictionary<GameResource, float>();
        for (int i = 0; i < structure.StructureBuildInfo.Cost.Length; i++) {
            if (!resources.ContainsKey(structure.StructureBuildInfo.Cost[i].Resource)) resources.Add(structure.StructureBuildInfo.Cost[i].Resource, 0);
            resources[structure.StructureBuildInfo.Cost[i].Resource] += GetResourceRefundAmount(structure, structure.StructureBuildInfo.Cost[i]);
        }

        int x = 0;
        foreach (KeyValuePair<GameResource, float> resource in resources) {
            if (Mathf.RoundToInt(resource.Value) == 0) continue;
            if (resource.Value >= 0) {
                resourceDisplays[x].AmountText.text = "+";
                resourceDisplays[x].AmountText.color = GameController.instance.PositiveColor;
            } 
            else {
                resourceDisplays[x].AmountText.text = "-";
                resourceDisplays[x].AmountText.color = GameController.instance.NegativeColor;
            }
            resourceDisplays[x].AmountText.text += Mathf.RoundToInt(resource.Value);
            resourceDisplays[x].Icon.sprite = resourceIcons[(int)resource.Key];
            resourceDisplays[x].Icon.color = new Color(255, 255, 255, 255);
            x++;
        }
    }

    private void ClearDisplays() {
        productionDisplay.AmountText.text = "";
        productionDisplay.Icon.color = new Color(255, 255, 255, 0);
        productionDisplay.ResourceTypeText.text = "";
        
        for (int i = 0; i < resourceDisplays.Length; i++) {
            resourceDisplays[i].AmountText.text = "";
            resourceDisplays[i].Icon.color = new Color(255, 255, 255, 0);
            if (resourceDisplays[i].ResourceTypeText) resourceDisplays[i].ResourceTypeText.text = "";
        }
    }

    private float GetResourceRefundAmount(Structure _structure, RBHKCost _cost) {
        if (_structure.ApplyFullRefunds) return _cost.Amount;
        if (_structure.FullRefundResources.Contains(_cost.Resource)) return _cost.Amount;
        //if (_structure.DontApplyRefunds && _structure.FullRefundResources.Contains(_cost.Resource)) return _cost.Amount;
        //if (_structure.DontApplyRefunds) return 0;
        return Mathf.Round(_cost.Amount * _structure.RefundPercentage);
    }

    #endregion
}
