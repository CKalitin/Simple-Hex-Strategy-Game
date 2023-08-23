using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayStructure : MonoBehaviour {
    #region Variables
    
    [Tooltip("Clockwise from Top Right")]
    [SerializeField] private TextMeshProUGUI[] bonusTexts;

    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;

    private GameplayStructure.Bonus[] bonuses;

    private Tile tile;

    public Tile Tile { get => tile; set => tile = value; }
    public GameplayStructure.Bonus[] Bonuses { get => bonuses; set => bonuses = value; }
    public TextMeshProUGUI[] BonusTexts { get => bonusTexts; set => bonusTexts = value; }

    #endregion

    #region Core

    private void Awake() {
        for (int i = 0; i < bonusTexts.Length; i++) {
            bonusTexts[i].text = "";
        }
    }

    private void Update() {
        // Tile is set in BuildManager.cs
        if (tile == null) return;

        int[] bonusValues = new int[6];

        // Loop through bonuses, then loop through directions and update the bonusValues array, then set the bonus text to the bonus value of its direction
        for (int i = 0; i < bonuses.Length; i++) {
            List<Vector2Int> activeDirections = GameUtils.GetDirectionsWithID(tile.Location, bonuses[i].BonusStructureID);
            for (int x = 0; x < GameUtils.Directions.Length; x++) {
                if (activeDirections.Contains(GameUtils.Directions[x])) {
                    bonusValues[x] += bonuses[i].BonusAmount;
                }
            }
        }
        
        for (int i = 0; i < bonusTexts.Length; i++) {
            if (bonusValues[i] == 0) { bonusTexts[i].text = ""; continue; }

            if (bonusValues[i] > 0) {
                bonusTexts[i].color = positiveColor;
                bonusTexts[i].text = "+";
            } else {
                bonusTexts[i].color = negativeColor;
                bonusTexts[i].text = "-";
            }
            
            bonusTexts[i].text = bonusTexts[i].text + bonusValues[i].ToString();
        }
    }

    #endregion
}
