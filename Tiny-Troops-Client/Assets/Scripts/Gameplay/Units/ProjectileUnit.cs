using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileUnit : MonoBehaviour {
    #region Variables

    [Header("Config")]
    [SerializeField] private float attackRate = 2.5f;
    [Tooltip("How long a single attack takes. The time from projectile shot to hit.")]
    [SerializeField] private float attackTime = 0.75f;
    [SerializeField] private float attackDamage;
    [Space]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPos;

    private PathfindingAgent pathfindingAgent;
    private Unit unit;

    public Vector2Int Location { get => pathfindingAgent.CurrentTile; }
    public int PlayerID { get => unit.PlayerID; }

    #endregion

    #region Core

    private void Awake() {
        pathfindingAgent = GetComponent<PathfindingAgent>();
        unit = GetComponent<Unit>();
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

            ShootProjectile(new Vector2(unit.transform.position.x, transform.position.z + (transform.position.z - unit.transform.position.z)));
        }
    }

    private void ShootProjectile(Vector2 _pos) {
        // Get Quaternion Direction to unit from here
        Vector2 direction = _pos - new Vector2(transform.position.x, transform.position.z);
        float dist = Vector2.Distance(_pos, new Vector2(transform.position.x, transform.position.z));
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject p = Instantiate(projectilePrefab, new Vector3(projectileSpawnPos.position.x, transform.position.y, projectileSpawnPos.position.z), Quaternion.AngleAxis(angle, Vector3.up));

        p.GetComponent<Animator>().speed = 1f / attackTime;
        p.GetComponent<Transform>().localScale = new Vector3(dist, projectileSpawnPos.position.y - transform.position.y, 1f);
        Destroy(p, attackTime + 0.1f);
    }

    #endregion

    #region Utils

    private Unit GetClosestUnit() {
        List<Unit> units = GetUnitsOnAdjacentTiles();
        if (units.Count == 0) return null;

        Unit closestUnit = units[0];
        float closestDistance = Vector3.Distance(transform.position, closestUnit.transform.position);

        for (int i = 1; i < units.Count; i++) {
            if (units[i].gameObject == null) continue;
            float distance = Vector3.Distance(transform.position, units[i].transform.position);
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
