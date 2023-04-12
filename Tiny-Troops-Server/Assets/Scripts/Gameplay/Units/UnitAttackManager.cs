using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitAttackManager : MonoBehaviour {
    #region Variables

    [SerializeField] private float attackTickTime;

    private float totalDeltaTime = 0;
    private int totalTicks;

    private Dictionary<Vector2Int, Dictionary<int, PlayerTileUnitInfo>> tileAttackInfo;

    private class PlayerTileUnitInfo {
        private Vector2Int location;

        private List<UnitInfo> units; // Sorted by health
        private float unitAttackDamage;
        private float structureAttackDamage;
        private float totalHealth;

        public Vector2Int Location { get => location; set => location = value; }
        public List<UnitInfo> Units { get => units; set => units = value; }
        public float UnitAttackDamage { get => unitAttackDamage; set => unitAttackDamage = value; }
        public float StructureAttackDamage { get => structureAttackDamage; set => structureAttackDamage = value; }
        public float TotalHealth { get => totalHealth; set => totalHealth = value; }
    }

    #endregion

    #region Core

    private void Update() {
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
        // Update before attack - Thing's have changed in the tickTime since last execution
        SetTileAttackInfo();

        foreach (Vector2Int location in tileAttackInfo.Keys) {
            foreach (int playerID in tileAttackInfo[location].Keys) {
                bool attacking = false;

                // Attack enemy player's units on tile
                foreach (int defendingPlayerID in tileAttackInfo[location].Keys) {
                    if (playerID != defendingPlayerID) AttackUnitsOnTile(location, playerID, defendingPlayerID);
                }
                AttackStructureOnTile(location, playerID);

                if (tileAttackInfo[location].Values.Count > 1) attacking = true; // If more than 1 player's units on tile, attack
                if (TileManagement.instance.GetTileAtLocation(location).Tile.Structures.Count > 0 && TileManagement.instance.GetTileAtLocation(location).Tile.Structures[0].PlayerID != playerID)
                    attacking = true; // If enemy structure on tile, attack

                if (attacking) UnitManager.instance.GetUnitsOfIdAtLocation(location, playerID).ForEach(x => UnitManager.instance.Units[x].Script.Attacking = true);
            }
        }

        // Update after attack so the player's have accurate information
        SetTileAttackInfo();
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
        if (TileManagement.instance.GetTileAtLocation(_location).Tile.Structures[0].PlayerID == _attackingPlayerID) return false;
        
        TileManagement.instance.GetTileAtLocation(_location).Tile.Structures[0].GetComponent<Health>().ChangeHealth(-Mathf.Abs(tileAttackInfo[_location][_attackingPlayerID].StructureAttackDamage));

        return true;
    }

    private void SetTileAttackInfo() {
        tileAttackInfo = new Dictionary<Vector2Int, Dictionary<int, PlayerTileUnitInfo>>();

        foreach (UnitInfo unit in UnitManager.instance.Units.Values) {
            Vector2Int location = unit.Location;

            if (!tileAttackInfo.ContainsKey(location))
                tileAttackInfo.Add(location, new Dictionary<int, PlayerTileUnitInfo>());

            if (!tileAttackInfo[location].ContainsKey(unit.PlayerID)) {
                tileAttackInfo[location].Add(unit.PlayerID, new PlayerTileUnitInfo());
                tileAttackInfo[location][unit.PlayerID].Location = location;
                tileAttackInfo[location][unit.PlayerID].Units = new List<UnitInfo>();
            }

            tileAttackInfo[location][unit.PlayerID].Units.Add(unit);

            bool unitsPresent = UnitManager.instance.GetPlayerUnitsAtLocation(location).Count > 1; // If enemy units present
            bool structurePresent = TileManagement.instance.GetTileAtLocation(location).Tile.Structures.Count > 0;
            
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

            tileAttackInfo[location][unit.PlayerID].TotalHealth += unit.Script.Health.CurrentHealth;
        }

        // Sort units on each tile by health
        foreach (KeyValuePair<Vector2Int, Dictionary<int, PlayerTileUnitInfo>> kvp in tileAttackInfo) {
            foreach (KeyValuePair<int, PlayerTileUnitInfo> x in tileAttackInfo[kvp.Key]) {
                x.Value.Units.OrderBy(d => d.Script.Health.CurrentHealth);
            }
        }
    }

    #endregion
}