using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathfindingAgent : MonoBehaviour {
    #region Variables

    [SerializeField] private float moveSpeed = 1f;
    [Space]
    [Tooltip("When location is set by server, how fast does the agent lerp there.")]
    [SerializeField] private float lerpToLocationSpeed = 0.5f;

    private Vector2Int currentPathfindingLocation;
    private Vector2Int currentTile;

    private Vector3 startPos;
    private Vector3 targetPos;

    private List<Vector2Int> path;

    private float previousDistance = 999999999f;

    private bool finishedMoving = true;
    private bool lerpingToLocation = false;

    private int randomSeed = 0;
    private System.Random random;

    public Vector2Int CurrentPathfindingLocation { get => currentPathfindingLocation; set => currentPathfindingLocation = value; }
    public Vector2Int CurrentTile { get => currentTile; set => currentTile = value; }
    public bool FinishedMoving { get => finishedMoving; set => finishedMoving = value; }

    public Vector2Int TargetLocation { get => path[path.Count - 1]; }

    #endregion

    #region Core

    private void Awake() {
        if (randomSeed == 0) randomSeed = UnityEngine.Random.Range(1, 999999999);
    }

    private void Update() {
        Move();
    }

    public void Initialize(Vector2Int _spawnLocation, int _randomSeed) {
        path = null;
        finishedMoving = true;

        currentPathfindingLocation = _spawnLocation;

        randomSeed = _randomSeed;
        random = new System.Random(_randomSeed);
    }

    #endregion

    #region Movement

    private void Move() {
        if (finishedMoving || lerpingToLocation) return;

        Vector3 direction = transform.position + (targetPos - startPos).normalized;
        direction.y = transform.position.y;

        Vector3 newPos = Vector3.MoveTowards(transform.position, direction, moveSpeed * Time.deltaTime);
        transform.position = newPos;

        if (Vector3.Distance(new Vector3(transform.position.x, targetPos.y, transform.position.z), targetPos) > previousDistance) ReachedTargetPos();
        previousDistance = Vector3.Distance(new Vector3(transform.position.x, targetPos.y, transform.position.z), targetPos);
    }

    private void ReachedTargetPos() {
        currentPathfindingLocation = path[0];
        currentTile = PathfindingManager.instance.PathfindingLocationsMap[path[0]].Tile.Location;

        path.RemoveAt(0);
        if (path.Count <= 0) {
            finishedMoving = true;
            return;
        }

        // Check if next location is not walkable
        if (PathfindingManager.instance.PathfindingLocationsMap[path[0]].Walkable == false) {
            PathfindToLocation(path[path.Count - 1]);
            return;
        }

        startPos = transform.position;
        targetPos = PathfindingManager.instance.PathfindingLocationsMap[path[0]].transform.position + GetNextRandomLocation(PathfindingManager.instance.PathfindingLocationsMap[path[0]].Radius);
    }

    #endregion

    #region Public Functions

    public void PathfindToLocation(Vector2Int _targetLocation, List<Vector2Int> _path = null) {
        if (_path != null) path = _path;
        else path = PathfindingManager.FindPath(currentPathfindingLocation, _targetLocation);

        // If there's nowhere to move
        if (path.Count <= 1) path = null;
        if (path == null) return;

        path.RemoveAt(0); // Remove currentLocation

        startPos = transform.position;
        targetPos = PathfindingManager.instance.PathfindingLocationsMap[path[0]].transform.position + GetNextRandomLocation(PathfindingManager.instance.PathfindingLocationsMap[path[0]].Radius); ;

        previousDistance = 999999999f;
        finishedMoving = false;
    }

    public void SetLocation(Vector2Int _location, Vector2 _pos) {
        if (_location == currentPathfindingLocation) return;

        lerpingToLocation = true;

        path = null;
        finishedMoving = true;

        currentPathfindingLocation = _location;
        currentTile = PathfindingManager.instance.PathfindingLocationsMap[_location].Tile.Location;

        startPos = _pos;
        targetPos = _pos;

        previousDistance = 999999999f;

        StartCoroutine(LerpToLocation(_pos));
    }

    #endregion

    #region Helper Functions

    private IEnumerator LerpToLocation(Vector3 _targetPostion) {
        Vector3 initialPos = transform.position;
        Vector3 endPos = new Vector3(_targetPostion.x, transform.position.y, _targetPostion.z);
        float lerpTime = 0;

        while (Vector3.Distance(transform.position, endPos) >= 0.01f) {
            transform.position = Vector3.Lerp(initialPos, endPos, lerpTime);
            lerpTime += Mathf.Clamp(Time.deltaTime * lerpToLocationSpeed, 0, 1);
            yield return new WaitForEndOfFrame();
        }

        startPos = _targetPostion;
        lerpingToLocation = false;
    }

    private Vector3 GetNextRandomLocation(float _radius) {
        return new Vector3(GameUtils.Random(ref random, -_radius, _radius), 0, GameUtils.Random(ref random, -_radius, _radius));
    }

    #endregion
}
