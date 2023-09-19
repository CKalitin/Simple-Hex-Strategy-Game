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

    private Vector2Int currentLocation;
    private Vector2Int currentTile;

    private Vector3 offset = Vector3.zero;

    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 nextPos; // After Target Pos

    private List<Vector2Int> path;

    private float currentLerp = 0f;

    private bool finishedMoving = true;
    private bool lerpingToLocation = false;

    private int randomSeed = 0;
    private System.Random random;

    public Vector2Int CurrentLocation { get => currentLocation; set => currentLocation = value; }
    public Vector2Int CurrentTile { get => currentTile; set => currentTile = value; }
    public bool FinishedMoving { get => finishedMoving; set => finishedMoving = value; }
    public int RandomSeed { get => randomSeed; set => randomSeed = value; }
    public System.Random Random { get => random; set => random = value; }

    public Vector2Int GetTargetLocation() {
        if (path == null) return currentTile;
        if (path.Count > 0) return PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[path.Count - 1])].Tile.Location;
        return currentTile;
    }

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

        currentLocation = _spawnLocation;
        CurrentTile = PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(_spawnLocation)].Tile.Location;

        randomSeed = _randomSeed;
        random = new System.Random(_randomSeed);
    }

    #endregion

    #region Movement

    private void Move() {
        if (finishedMoving || lerpingToLocation) return;
        if (path == null) {
            // Print every global variable - Bug fixing
            Debug.Log("Current Location: " + currentLocation);
            Debug.Log("Current Tile: " + currentTile);
            Debug.Log("Finished Moving: " + finishedMoving);
            Debug.Log("Lerping To Location: " + lerpingToLocation);
            Debug.Log("Start Pos: " + startPos);
            Debug.Log("Target Pos: " + targetPos);
            Debug.Log("Next Pos: " + nextPos);
            Debug.Log("Path: " + string.Join(", ", path));
            Debug.Log("Current Lerp: " + currentLerp);

            finishedMoving = true;
            currentLerp = 0f;

            return;
        }

        currentLerp = currentLerp + (moveSpeed * Time.deltaTime);

        Vector3 nextFramePos = Vector3.zero;

        // Bezier
        // The if statement is necessary because the agent may pathfind one unit, in this situation the bezier curve will not work
        if (targetPos != nextPos) {
            Vector3 lerp1 = Vector3.Lerp(startPos, targetPos, currentLerp);
            Vector3 lerp2 = Vector3.Lerp(targetPos, nextPos, currentLerp);
            transform.position = Vector3.Lerp(lerp1, lerp2, currentLerp);
            nextFramePos = Vector3.Lerp(lerp1, lerp2, currentLerp + 0.05f);
        } else {
            transform.position = Vector3.Lerp(startPos, targetPos, currentLerp);
            nextFramePos = Vector3.Lerp(startPos, targetPos, currentLerp + 0.05f);
        }

        // Rotate to direction of movement
        Vector2 direction = new Vector2(nextFramePos.x, nextFramePos.z) - new Vector2(transform.position.x, transform.position.z);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);

        if (path.Count <= 2 && currentLerp >= 1f) { // Almost reached end of path
            if (path.Count > 1) { path.RemoveAt(0); }
            ReachedTargetPos();
        } else if (path.Count > 2 && currentLerp >= 0.5f) ReachedTargetPos(); // Reached half way through current lerp - Set new lerp positions
    }

    private void ReachedTargetPos() {
        currentLerp = 0f;
        
        if (path == null) {
            finishedMoving = true;
            return;
        }

        currentLocation = path[0];
        currentTile = PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[0])].Tile.Location;

        path.RemoveAt(0);
        if (path.Count <= 0) {
            finishedMoving = true;
            return;
        }

        // Check if next location is not walkable
        if (PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[0])].Walkable == false) {
            PathfindToLocation(path[path.Count - 1]);
            return;
        }

        startPos = transform.position;
        targetPos = PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[0])].transform.position + GetNextRandomLocation(PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[0])].Radius);
        nextPos = PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[1])].transform.position + GetNextRandomLocation(PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[1])].Radius);
    }

    #endregion

    #region Public Functions

    public void PathfindToLocation(Vector2Int _targetLocation, List<Vector2Int> _path = null) {
        if (_path != null) path = _path;
        else path = PathfindingManager.FindPath(currentLocation, _targetLocation);

        currentLerp = 0f;

        // If there's nowhere to move
        if (path.Count <= 1) path = null;
        if (path == null) return;
        
        path.RemoveAt(0); // Remove currentLocation

        startPos = transform.position;
        targetPos = PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[0])].transform.position + GetNextRandomLocation(PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[0])].Radius);

        if (path.Count > 1) nextPos = PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[1])].transform.position + GetNextRandomLocation(PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(path[1])].Radius);
        else nextPos = targetPos;
        
        finishedMoving = false;
    }

    public void SetLocation(Vector2Int _location, Vector3 _pos) {
        if (_location == currentLocation) return;

        currentLerp = 0f;
        
        lerpingToLocation = true;

        path = null;
        finishedMoving = true;

        currentLocation = _location;
        currentTile = PathfindingManager.instance.PathfindingLocationsMap[PathfindingManager.RBHKLocationToPathfindingLocation(_location)].Tile.Location;

        startPos = _pos;
        targetPos = _pos;
        nextPos = _pos;

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
        if (offset == Vector3.zero) offset = new Vector3(UnityEngine.Random.Range(-_radius, _radius), 0, UnityEngine.Random.Range(-_radius, _radius));
        return offset + new Vector3(UnityEngine.Random.Range(-_radius / 50, _radius / 50), 0, UnityEngine.Random.Range(-_radius / 50, _radius / 50));
    }

    #endregion
}
