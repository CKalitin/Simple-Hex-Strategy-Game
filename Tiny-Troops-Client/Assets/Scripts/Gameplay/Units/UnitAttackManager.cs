using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitAttackManager : MonoBehaviour {
    #region Varibales

    public static UnitAttackManager instance;

    [SerializeField] private float attackTickTime;

    private float totalDeltaTime = 0;
    private int totalTicks;

    private Dictionary<Vector2Int, Dictionary<int, PlayerTileUnitInfo>> tileAttackInfo = new Dictionary<Vector2Int, Dictionary<int, PlayerTileUnitInfo>>();
    private Dictionary<int, PlayerUnitInfo> playerUnitInfos = new Dictionary<int, PlayerUnitInfo>();
    private PlayerUnitInfo playerSelectedUnitInfo;
    
    public Dictionary<Vector2Int, Dictionary<int, PlayerTileUnitInfo>> TileAttackInfo { get => tileAttackInfo; set => tileAttackInfo = value; }
    public Dictionary<int, PlayerUnitInfo> PlayerUnitInfos { get => playerUnitInfos; set => playerUnitInfos = value; }
    public PlayerUnitInfo PlayerSelectedUnitInfo { get => playerSelectedUnitInfo; set => playerSelectedUnitInfo = value; }

    public class PlayerUnitInfo {
        private List<int> numUnitsByType = new List<int>();

        private float totalMaxHealth;
        private float totalCurrentHealth;
        private float totalUnitAttackDamage;
        private float totalStructureAttackDamage;

        public PlayerUnitInfo() {
            numUnitsByType = new List<int>() { 0, 0 };
        }

        public List<int> NumUnitsByType { get => numUnitsByType; set => numUnitsByType = value; }
        public float TotalMaxHealth { get => totalMaxHealth; set => totalMaxHealth = value; }
        public float TotalCurrentHealth { get => totalCurrentHealth; set => totalCurrentHealth = value; }
        public float TotalUnitAttackDamage { get => totalUnitAttackDamage; set => totalUnitAttackDamage = value; }
        public float TotalStructureAttackDamage { get => totalStructureAttackDamage; set => totalStructureAttackDamage = value; }
    }

    public class PlayerTileUnitInfo {
        private Vector2Int location;

        private List<UnitInfo> units = new List<UnitInfo>(); // Sorted by health
        private List<int> numUnitsByType = new List<int>();

        private float totalMaxHealth;
        private float totalCurrentHealth;
        private float unitAttackDamage;
        private float structureAttackDamage;

        private float potentialUnitAttackDamage;
        private float potentialStructureAttackDamage;

        public PlayerTileUnitInfo() {
            numUnitsByType = new List<int>() { 0, 0 };
        }

        public Vector2Int Location { get => location; set => location = value; }
        public List<UnitInfo> Units { get => units; set => units = value; }
        public List<int> NumUnitsByType { get => numUnitsByType; set => numUnitsByType = value; }
        public float TotalMaxHealth { get => totalMaxHealth; set => totalMaxHealth = value; }
        public float TotalCurrentHealth { get => totalCurrentHealth; set => totalCurrentHealth = value; }
        public float UnitAttackDamage { get => unitAttackDamage; set => unitAttackDamage = value; }
        public float StructureAttackDamage { get => structureAttackDamage; set => structureAttackDamage = value; }
        public float PotentialUnitAttackDamage { get => potentialUnitAttackDamage; set => potentialUnitAttackDamage = value; }
        public float PotentialStructureAttackDamage { get => potentialStructureAttackDamage; set => potentialStructureAttackDamage = value; }
    }

    #endregion

    #region Core

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (FindObjectsOfType<TileManagement>().Length > 1) {
            Debug.Log($"Unit Attack Manager already exists on {gameObject}", gameObject);
            Destroy(this);
        } else {
            instance = this;
        }
    }

    private void Start() {
        SetTileAttackInfo();
    }

    private void Update() {
        SetTileAttackInfo();
        TickUpdate();
    }

    private void TickUpdate() {
        totalDeltaTime += Time.deltaTime;
        float deltaTimeDifference = totalDeltaTime - (totalTicks * attackTickTime);

        if (deltaTimeDifference >= attackTickTime) {
            totalTicks++;
            AttackUpdate();
        }
    }
    
    private void AttackUpdate() {
        foreach (Vector2Int location in tileAttackInfo.Keys) {
            foreach (int playerID in tileAttackInfo[location].Keys) {
                bool attacking = false;

                // This code is only necessary on the Server
                // Attack enemy player's units on tile
                /*foreach (int defendingPlayerID in tileAttackInfo[location].Keys) {
                    if (playerID != defendingPlayerID) AttackUnitsOnTile(location, playerID, defendingPlayerID);
                }
                AttackStructureOnTile(location, playerID);*/

                if (tileAttackInfo[location].Values.Count > 1) attacking = true; // If more than 1 player's units on tile, attack
                if (TileManagement.instance.GetTileAtLocation(location).Tile.Structures.Count > 0 && TileManagement.instance.GetTileAtLocation(location).Tile.Structures[0].PlayerID != playerID)
                    attacking = true; // If enemy structure on tile, attack

                if (attacking) UnitManager.instance.GetUnitsOfIdAtLocation(location, playerID).ForEach(x => UnitManager.instance.Units[x].Script.Attacking = true);
            }
        }
    }

    #endregion

    #region Unit Attack Manager

    private void AttackUnitsOnTile(Vector2Int _location, int _attackingPlayerID, int _defendingPlayerID) {
        PlayerTileUnitInfo attackingPlayer = tileAttackInfo[_location][_attackingPlayerID];
        PlayerTileUnitInfo defendingPlayer = tileAttackInfo[_location][_defendingPlayerID];

        if (attackingPlayer.UnitAttackDamage > 0) {
            float damageToBeDealt = attackingPlayer.UnitAttackDamage;

            for (int i = 0; i < defendingPlayer.Units.Count; i++) {
                if (defendingPlayer.Units[i].Script.Health.CurrentHealth > damageToBeDealt / tileAttackInfo[_location].Keys.Count) {
                    defendingPlayer.Units[i].Script.Health.ChangeHealth(-Mathf.Abs(damageToBeDealt / tileAttackInfo[_location].Keys.Count));
                    break;
                } else {
                    defendingPlayer.Units[i].Script.Health.SetHealth(0f);
                    damageToBeDealt -= defendingPlayer.Units[i].Script.Health.CurrentHealth;
                }
            }
        }
    }

    // _attack is used so the Client and Server can have similar code
    // The client doesn't attack, but it needs to know if an attack is possible so the player can see it
    private bool AttackStructureOnTile(Vector2Int _location, int _attackingPlayerID) {
        if (TileManagement.instance.GetTileAtLocation(_location).Tile.Structures.Count <= 0) return false;
        if (TileManagement.instance.GetTileAtLocation(_location).Tile.Structures[0].PlayerID < 0) return false;
        if (TileManagement.instance.GetTileAtLocation(_location).Tile.Structures[0].PlayerID == _attackingPlayerID) return false;
        
        TileManagement.instance.GetTileAtLocation(_location).Tile.Structures[0].GetComponent<Health>().ChangeHealth(-Mathf.Abs(tileAttackInfo[_location][_attackingPlayerID].StructureAttackDamage));

        return true;
    }

    private void SetTileAttackInfo() {
        tileAttackInfo = new Dictionary<Vector2Int, Dictionary<int, PlayerTileUnitInfo>>();
        playerUnitInfos = new Dictionary<int, PlayerUnitInfo>();
        playerSelectedUnitInfo = new PlayerUnitInfo();

        foreach (UnitInfo unit in UnitManager.instance.Units.Values) {
            Vector2Int location = unit.Location;

            if (!tileAttackInfo.ContainsKey(location))
                tileAttackInfo.Add(location, new Dictionary<int, PlayerTileUnitInfo>());

            if (!tileAttackInfo[location].ContainsKey(unit.PlayerID)) {
                tileAttackInfo[location].Add(unit.PlayerID, new PlayerTileUnitInfo());
                tileAttackInfo[location][unit.PlayerID].Location = location;
                tileAttackInfo[location][unit.PlayerID].Units = new List<UnitInfo>();
            }

            if (!playerUnitInfos.ContainsKey(unit.PlayerID))
                playerUnitInfos.Add(unit.PlayerID, new PlayerUnitInfo());

            tileAttackInfo[location][unit.PlayerID].Units.Add(unit);
            TileAttackInfo[location][unit.PlayerID].NumUnitsByType[0]++;

            bool unitsPresent = UnitManager.instance.GetPlayerUnitsAtLocation(location).Count > 1; // If enemy units present
            bool structurePresent = TileManagement.instance.GetTileAtLocation(location).Tile.Structures.Count > 0 && TileManagement.instance.GetTileAtLocation(location).Tile.Structures[0].PlayerID != -1;

            // If only units are present
            if (unitsPresent && !structurePresent) {
                tileAttackInfo[location][unit.PlayerID].UnitAttackDamage += unit.Script.UnitAttackDamage;
            }
            // If there is only a structure on tile
            else if (!unitsPresent && structurePresent) {
                tileAttackInfo[location][unit.PlayerID].StructureAttackDamage += unit.Script.StructureAttackDamage;
            }
            // If either units and structures or nothing
            else {
                if (unit.Script.AttackPriority == AttackPriority.Units)
                    tileAttackInfo[location][unit.PlayerID].UnitAttackDamage += unit.Script.UnitAttackDamage;
                else
                    tileAttackInfo[location][unit.PlayerID].StructureAttackDamage += unit.Script.StructureAttackDamage;
            }

            tileAttackInfo[location][unit.PlayerID].PotentialUnitAttackDamage += unit.Script.UnitAttackDamage;
            tileAttackInfo[location][unit.PlayerID].PotentialStructureAttackDamage += unit.Script.StructureAttackDamage;

            tileAttackInfo[location][unit.PlayerID].TotalCurrentHealth += unit.Script.Health.CurrentHealth;
            tileAttackInfo[location][unit.PlayerID].TotalMaxHealth += unit.Script.Health.CurrentHealth;

            playerUnitInfos[unit.PlayerID].NumUnitsByType[0]++;
            playerUnitInfos[unit.PlayerID].TotalUnitAttackDamage += unit.Script.UnitAttackDamage;
            playerUnitInfos[unit.PlayerID].TotalStructureAttackDamage += unit.Script.StructureAttackDamage;
            playerUnitInfos[unit.PlayerID].TotalMaxHealth += unit.Script.GetComponent<Health>().MaxHealth;
            playerUnitInfos[unit.PlayerID].TotalCurrentHealth += unit.Script.GetComponent<Health>().CurrentHealth;

            if (UnitSelector.instance.SelectedUnits.ContainsKey(unit.Script.UnitUUID)) {
                playerSelectedUnitInfo.NumUnitsByType[0]++;
                playerSelectedUnitInfo.TotalUnitAttackDamage += unit.Script.UnitAttackDamage;
                playerSelectedUnitInfo.TotalStructureAttackDamage += unit.Script.StructureAttackDamage;
                playerSelectedUnitInfo.TotalMaxHealth += unit.Script.GetComponent<Health>().MaxHealth;
                playerSelectedUnitInfo.TotalCurrentHealth += unit.Script.GetComponent<Health>().CurrentHealth;
            }
        }

        // Sort units on each tile by health
        foreach (KeyValuePair<Vector2Int, Dictionary<int, PlayerTileUnitInfo>> kvp in tileAttackInfo) {
            foreach (KeyValuePair<int, PlayerTileUnitInfo> x in tileAttackInfo[kvp.Key]) {
                x.Value.Units.OrderBy(d => d.Script.Health.CurrentHealth);
            }
        }
    }

    public void ResetManager() {
        totalDeltaTime = 0f;
        totalTicks = 0;

        tileAttackInfo = new Dictionary<Vector2Int, Dictionary<int, PlayerTileUnitInfo>>();
    }

    #endregion
}
