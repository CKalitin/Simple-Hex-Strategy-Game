using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class DisplayStructure : MonoBehaviour {
    #region Variables
    
    [Tooltip("Clockwise from Top Right")]
    [SerializeField] private TextMeshProUGUI[] bonusTexts;
    [SerializeField] private TextMeshProUGUI centerBonusText;
    [Space]
    [SerializeField] private Color positiveColor = Color.green;
    [SerializeField] private Color negativeColor = Color.red;
    [Space]
    [SerializeField] private GameObject outOfVillageRangeDisplay;

    private GameplayStructure.Bonus[] bonuses;

    private Tile tile;

    private bool withinRangeOfVillage = true;

    public Tile Tile { get => tile; set => tile = value; }
    public GameplayStructure.Bonus[] Bonuses { get => bonuses; set => bonuses = value; }
    public TextMeshProUGUI[] BonusTexts { get => bonusTexts; set => bonusTexts = value; }
    public bool WithinRangeOfVillage { get => withinRangeOfVillage; set => withinRangeOfVillage = value; }

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

        CheckVillageRange();
        outOfVillageRangeDisplay.SetActive(!withinRangeOfVillage);

        if (!withinRangeOfVillage) {
            for (int i = 0; i < bonusTexts.Length; i++) bonusTexts[i].text = "";
            centerBonusText.text = "";
            return;
        }

        int[] bonusValues = new int[6];

        if (ClientStructureBuilder.instance.StructureBuildInfos[(int)BuildManager.instance.CurrentStructureBuildInfo.StructurePrefab.GetComponent<ConstructionStructure>().ConstructedStructureID].StructurePrefab.GetComponent<Structure>().ResourceEntries.Length > 0) {
            centerBonusText.text = "+" + ClientStructureBuilder.instance.StructureBuildInfos[(int)BuildManager.instance.CurrentStructureBuildInfo.StructurePrefab.GetComponent<ConstructionStructure>().ConstructedStructureID].StructurePrefab.GetComponent<Structure>().ResourceEntries[0].Change.ToString();
        } else {
            centerBonusText.text = "";
        }

        // Loop through bonuses, then loop through directions and update the bonusValues array, then set the bonus text to the bonus value of its direction
        for (int i = 0; i < bonuses.Length; i++) {
            List<Vector2Int> activeDirections = GameUtils.GetDirectionsWithID(tile.Location, bonuses[i].BonusStructureID);
            for (int x = 0; x < GameUtils.Directions.Length; x++) {
                if (activeDirections.Contains(GameUtils.Directions[x])) {
                    bonusValues[x] += bonuses[i].BonusAmount;
                }
                bonusValues[x] += GetExtraBonusForNearbyTile(tile.Location, GameUtils.GetTargetDirection(tile.Location, GameUtils.Directions[x]));
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

    private int GetExtraBonusForNearbyTile(Vector2Int _loc, Vector2Int _nearbyTileLoc) {
        Tile nearbyTile = TileManagement.instance.GetTileAtLocation(_nearbyTileLoc).Tile;
        GameplayStructure nearbyGS;
        int output = 0;
        if (nearbyTile.Structures.Count <= 0 || (nearbyGS = nearbyTile.Structures[0].GetComponent<GameplayStructure>()) == null) return 0;
        
        for (int i = 0; i < nearbyGS.Bonuses.Length; i++) {
            if (nearbyGS.Bonuses[i].BonusStructureID == BuildManager.instance.CurrentStructureBuildInfo.StructurePrefab.GetComponent<ConstructionStructure>().ConstructedStructureID) {
                output += nearbyGS.Bonuses[i].BonusAmount;
            }
        }
        return output;
    }

    private void CheckVillageRange() {
        List<Vector2Int> adjacentTileLocations = TileManagement.instance.GetAdjacentTilesInRadius(tile.Location, BuildManager.instance.VillageRange);
        withinRangeOfVillage = adjacentTileLocations.Contains(GetClosestVillages(MatchManager.instance.PlayerID, tile.Location)[0].Location);
    }

    private List<ClientVillage> GetClosestVillages(int _playerID, Vector2Int _location) {
        ClientVillage[] clientVillages = FindObjectsOfType<ClientVillage>();
        List<ClientVillage> playerVillages = new List<ClientVillage>();
        for (int i = 0;i < clientVillages.Length; i++) {
            if (clientVillages[i].GetComponent<Structure>().PlayerID == _playerID) {
                playerVillages.Add(clientVillages[i]);
            }
        }

        if (playerVillages.Count <= 0) return null;

        playerVillages = playerVillages.OrderBy(x => Vector3.Distance(TileManagement.instance.TileLocationToWorldPosition(_location, 0), TileManagement.instance.TileLocationToWorldPosition(x.Location, 0))).ToList();

        return playerVillages;
    }

    #endregion
}
