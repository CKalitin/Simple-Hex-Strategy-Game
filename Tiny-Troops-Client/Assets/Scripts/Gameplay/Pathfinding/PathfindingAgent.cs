using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingAgent : MonoBehaviour {
    #region Variables

    [SerializeField] private float moveSpeed = 1f;
    [Space]
    [Tooltip("When location is set by server, how fast does the agent lerp there.")]
    [SerializeField] private float lerpToLocationSpeed = 0.5f;

    private Vector2Int currentLocation;
    private Vector2Int targetLocation;

    private PathfindingNode currentNode;
    private PathfindingNode targetNode;

    private Vector3 startPos;
    private Vector3 targetPos;

    private List<Vector2Int> path;

    //    (0, 1)  (1, 1)
    // (-1,0) (self) (1, 0)
    //    (0,-1)  (1,-1)
    private Vector2Int targetDirection;
    private bool finishedMoving = true;
    private Vector2Int[] directions = new Vector2Int[6] { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(0, 1) };

    private float previousDistance = 999999999f;

    private int randomSeed = 0;

    private System.Random random;

    public Vector2Int CurrentLocation { get => currentLocation; set => currentLocation = value; }
    public Vector2Int TargetLocation { get => targetLocation; set => targetLocation = value; }
    public PathfindingNode CurrentNode { get => currentNode; set => currentNode = value; }
    public bool FinishedMoving { get => finishedMoving; set => finishedMoving = value; }

    #endregion

    #region Core

    private void Awake() {
        if (randomSeed == 0) randomSeed = UnityEngine.Random.Range(0, 999999999);
    }

    private void Update() {
        Move();
    }

    public void Initialize(Vector2Int _currentLocation, PathfindingNode _currentNode, int _randomSeed) {
        currentLocation = _currentLocation;
        currentNode = _currentNode;
        targetNode = _currentNode;
        startPos = transform.position;
        targetPos = transform.position;

        path = null;
        targetDirection = Vector2Int.zero;
        finishedMoving = true;

        randomSeed = _randomSeed;

        random = new System.Random(_randomSeed);
    }

    #endregion

    #region Movement

    private void Move() {
        if (finishedMoving) return;
        if (targetNode == null || currentNode == null) {
            if (TileManagement.instance.GetTileAtLocation(currentLocation).Tile.GetComponent<GameplayTile>().GetTilePathfinding().PathfindingNodes.ContainsKey(GetTargetDirection(CurrentLocation, TargetLocation))) {
                targetNode = TileManagement.instance.GetTileAtLocation(currentLocation).Tile.GetComponent<GameplayTile>().GetTilePathfinding().PathfindingNodes[GetTargetDirection(CurrentLocation, TargetLocation)];
                MoveToTargetNode(targetNode);
            } else {
                targetNode = GetClosestNodeToPosition(transform.position);
                MoveToTargetNode(targetNode);
            }
        }
        Vector3 direction = transform.position + (targetPos - startPos).normalized;
        direction.y = transform.position.y;

        Vector3 newPos = Vector3.MoveTowards(transform.position, direction, moveSpeed * Time.deltaTime);
        transform.position = newPos;

        if (Vector3.Distance(new Vector3(transform.position.x, targetPos.y, transform.position.z), targetPos) > previousDistance) {
            ReachedTargetNode();
        }
        previousDistance = Vector3.Distance(new Vector3(transform.position.x, targetPos.y, transform.position.z), targetPos);
    }

    private void ReachedTargetNode() {
        // If reached final location
        if ((path == null || path.Count <= 0) && currentLocation == targetLocation) {
            currentLocation = targetLocation;
            currentNode = targetNode;

            startPos = transform.position;
            targetPos = transform.position;

            path = null;
            targetDirection = Vector2Int.zero;
            finishedMoving = true;

            previousDistance = 999999999f;

            return;
        }

        // If next node is on another tile
        if (targetNode.PathfindingNodes[targetDirection] == null && targetNode.FinalNode) {
            Vector2Int fromDirection = GetTargetDirection(targetLocation, currentLocation);

            MoveToTargetNode(TileManagement.instance.GetTileAtLocation(targetLocation).TileObject.transform.parent.GetComponent<GameplayTile>().GetTilePathfinding().PathfindingNodes[fromDirection]);

            currentLocation = targetLocation;

            if (path != null) path.RemoveAt(0);
            if (path.Count > 0) targetLocation = path[0];
            targetDirection = GetTargetDirection(currentLocation, targetLocation);
            return;
        }

        MoveToTargetNode(targetNode.PathfindingNodes[targetDirection]);
    }

    private void MoveToTargetNode(PathfindingNode _targetNode) {
        currentNode = targetNode;
        startPos = transform.position;

        targetNode = _targetNode;
        targetPos = targetNode.transform.position + new Vector3(GameUtils.Random(randomSeed, -targetNode.Radius, targetNode.Radius), 0, GameUtils.Random(randomSeed + 1, -targetNode.Radius, targetNode.Radius));

        previousDistance = 999999999f;
    }

    #endregion

    #region Public Functions
    
    public void PathfindToLocation(Vector2Int _targetLocation, List<Vector2Int> _path = null) {
        if (_path != null) path = _path;
        else path = PathfindingManager.FindPath(currentLocation, _targetLocation);
        
        if (path == null) return;
        if (path.Count <= 1) {
            targetLocation = currentLocation;
            path = null;
            MoveInRandomDirectionOnCurrentTile(targetNode);
            return;
        }

        path.RemoveAt(0);
        targetLocation = path[0];
        targetDirection = GetTargetDirection(currentLocation, targetLocation);
        finishedMoving = false;

        targetNode = currentNode;

        previousDistance = 999999999f;

        // Trying to find a bug
        try {
            bool a = currentNode.PathfindingNodes[targetDirection] != null;
        } catch (Exception _ex){
            Debug.Log($"Error:\nCurrent: {currentLocation}, Target: {targetLocation}, Direction: {targetDirection}\n{_ex}");
        }

        if (currentNode.PathfindingNodes[targetDirection] != null) {
            MoveToTargetNode(targetNode.PathfindingNodes[targetDirection]);
        } else {
            // If next node is on another tile
            if (targetNode.PathfindingNodes[targetDirection] == null && targetNode.FinalNode) {
                Vector2Int fromDirection = GetTargetDirection(targetLocation, currentLocation);

                MoveToTargetNode(TileManagement.instance.GetTileAtLocation(targetLocation).TileObject.transform.parent.GetComponent<GameplayTile>().GetTilePathfinding().PathfindingNodes[fromDirection]);

                path.RemoveAt(0);
                currentLocation = targetLocation;
                if (path.Count > 0) targetLocation = path[0];
                targetDirection = GetTargetDirection(currentLocation, targetLocation);
                
                return;
            }
        }
    }

    public void SetLocation(Vector2Int _location, int _nodeIndex, Vector2 _pos) {
        currentLocation = _location;
        targetLocation = _location;

        currentNode = TileManagement.instance.GetTileAtLocation(currentLocation).Tile.GetComponent<GameplayTile>().GetTilePathfinding().NodesOnTile[_nodeIndex];
        targetNode = currentNode;

        //transform.position = currentNode.transform.position + new Vector3(GameUtils.Random(randomSeed, -currentNode.Radius, currentNode.Radius), 0, GameUtils.Random(randomSeed, -currentNode.Radius, currentNode.Radius));
        Vector3 targetPostion = new Vector3(_pos.x, transform.position.y, _pos.y);

        startPos = transform.position;
        targetPos = transform.position;

        path = null;
        targetDirection = Vector2Int.zero;
        finishedMoving = true;

        previousDistance = 0;

        StartCoroutine(LerpToLocation(targetPostion));
    }

    #endregion

    #region Helper Functions

    private Vector2Int GetTargetDirection(Vector2Int _currentLocation, Vector2Int _targetLocation) {
        Vector2Int _targetDirection = _targetLocation - _currentLocation;
        if (_currentLocation.y % 2 == 0 && _targetDirection.y != 0) _targetDirection.x += 1;
        return _targetDirection;
    }

    private void MoveInRandomDirectionOnCurrentTile(PathfindingNode _startNode) {
        int iters = 0;
        while (true) {
            Vector2Int _targetDirection = directions[GameUtils.Random(randomSeed, 0, directions.Length)];
            if (_startNode.PathfindingNodes[_targetDirection] != null) {
                MoveToTargetNode(_startNode.PathfindingNodes[_targetDirection]);
                return;
            }
            iters++;
            if (iters > 10) break;
        }
    }

    private IEnumerator LerpToLocation(Vector3 _targetPostion) {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(_targetPostion.x, transform.position.y, _targetPostion.z);
        float lerpTime = 0;

        while (Vector3.Distance(transform.position, endPos) >= 0.01f) {
            transform.position = Vector3.Lerp(startPos, endPos, lerpTime);
            lerpTime += Mathf.Clamp(Time.deltaTime * lerpToLocationSpeed, 0, 1);
            yield return new WaitForEndOfFrame();
        }
    }

    private PathfindingNode GetClosestNodeToPosition(Vector3 _position) {
        PathfindingNode closestNode = null;
        float closestDistance = 999999999f;

        foreach (PathfindingNode node in TileManagement.instance.GetTileAtLocation(currentLocation).Tile.GetComponent<GameplayTile>().GetTilePathfinding().NodesOnTile) {
            float distance = Vector3.Distance(node.transform.position, _position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestNode = node;
            }
        }

        return closestNode;
    }

    #endregion
}
