using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStructure : MonoBehaviour {
    #region Variables

    [Header("Config")]
    [SerializeField] private float attackRate = 2.5f;
    [Tooltip("How long a single attack takes. The time from projectile shot to hit.")]
    [SerializeField] private float attackTime = 0.75f;
    [SerializeField] private float attackDamage = 40f;

    private Structure structure;

    public Vector2Int Location { get => structure.Tile.Location; }
    public int PlayerID { get => structure.PlayerID; }

    #endregion

    #region Core

    private void Awake() {
        structure = GetComponent<Structure>();
    }

    private void Start() {
        StartCoroutine(AttackLoop());
    }

    #endregion

    #region Attacking

    private IEnumerator AttackLoop() {
        while (true) {
            yield return new WaitForSeconds(attackRate);

            Unit unit;
            if ((unit = GetClosestUnit()) == null) continue;

            StartCoroutine(DealDamage(unit, attackDamage, attackTime));
        }
    }

    private IEnumerator DealDamage(Unit _unit, float _damage, float _delaySeconds) {
        yield return new WaitForSeconds(_delaySeconds);
        _unit.GetComponent<Health>().ChangeHealth(-Mathf.Abs(_damage));
    }

    #endregion

    #region Utils

    private Unit GetClosestUnit() {
        List<Unit> units = GetUnitsOnAdjacentTiles();
        if (units.Count == 0) return null;

        Unit closestUnit = units[0];
        float closestDistance = Vector2Int.Distance(Location, closestUnit.Location);

        for (int i = 1; i < units.Count; i++) {
            if (units[i].gameObject == null) continue;
            float distance = Vector2Int.Distance(Location, units[i].Location);
            if (distance < closestDistance) {
                closestUnit = units[i];
                closestDistance = distance;
            }
        }

        return closestUnit;
    }
    
    private List<Unit> GetUnitsOnAdjacentTiles() {
        List<Unit> units = new List<Unit>();

        List<Vector2Int> nearbyTiles = TileManagement.instance.GetAdjacentTilesInRadius(Location, 1);
        nearbyTiles.Add(Location);

        for (int i = 0; i < nearbyTiles.Count; i++) {
            if (!UnitAttackManager.instance.TileAttackInfo.ContainsKey(nearbyTiles[i])) continue;
            foreach (KeyValuePair<int, UnitAttackManager.PlayerTileUnitInfo> kvp in UnitAttackManager.instance.TileAttackInfo[nearbyTiles[i]]) {
                if (kvp.Key == PlayerID) continue;
                foreach (UnitInfo unitInfo in kvp.Value.Units)
                    units.Add(unitInfo.Script);
            }
        }

        return units;
    }

    #endregion
}
