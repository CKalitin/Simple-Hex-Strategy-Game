using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingAgent : MonoBehaviour {
    [SerializeField] private float moveSpeed = 1f;

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

    public Vector2Int CurrentLocation { get => currentLocation; set => currentLocation = value; }

    private void Start() {
        PathfindToLocation(new Vector2Int(4, 4));
    }

    private void Update() {
        Move();
    }

    public void Initialize(Vector2Int _currentLocation, PathfindingNode _currentNode) {
        currentLocation = _currentLocation;
        currentNode = _currentNode;
        targetNode = _currentNode;
        startPos = transform.position;
    }
    
    private void Move() {
        if (finishedMoving) return;
        if (targetNode == null || currentNode == null) {
            targetNode = TileManagement.instance.GetTileAtLocation(currentLocation).TileObject.GetComponent<GameplayTile>().TilePathfinding.PathfindingNodes[targetLocation - currentLocation];
            MoveToTargetNode(TileManagement.instance.GetTileAtLocation(currentLocation).TileObject.GetComponent<GameplayTile>().TilePathfinding.PathfindingNodes[targetLocation - currentLocation]);
        }
        
        Vector3 destination = transform.position + (targetPos - startPos).normalized;
        destination.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(new Vector3(transform.position.x, targetPos.y, transform.position.z), targetPos) <= 0.01f) {
            ReachedTargetNode();
        }
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
            
            return;
        }
        
        // If next node is on another tile
        if (targetNode.PathfindingNodes[targetDirection] == null && targetNode.FinalNode) {
            Vector2Int fromDirection = GetTargetDirection(targetLocation, currentLocation);
            
            MoveToTargetNode(TileManagement.instance.GetTileAtLocation(targetLocation).TileObject.transform.parent.GetComponent<GameplayTile>().TilePathfinding.PathfindingNodes[fromDirection]);

            path.RemoveAt(0);
            currentLocation = targetLocation;
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
        targetPos = targetNode.transform.position + new Vector3(Random.Range(-targetNode.Radius, targetNode.Radius), 0, Random.Range(-targetNode.Radius, targetNode.Radius));
    }
    
    public void PathfindToLocation(Vector2Int _targetLocation) {
        path = PathfindingManager.FindPath(currentLocation, _targetLocation);
        
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

        if (currentNode.PathfindingNodes[targetDirection] != null) {
            MoveToTargetNode(currentNode);
        } else {
            // If next node is on another tile
            if (targetNode.PathfindingNodes[targetDirection] == null && targetNode.FinalNode) {
                Vector2Int fromDirection = GetTargetDirection(targetLocation, currentLocation);

                MoveToTargetNode(TileManagement.instance.GetTileAtLocation(targetLocation).TileObject.transform.parent.GetComponent<GameplayTile>().TilePathfinding.PathfindingNodes[fromDirection]);

                path.RemoveAt(0);
                currentLocation = targetLocation;
                if (path.Count > 0) targetLocation = path[0];
                targetDirection = GetTargetDirection(currentLocation, targetLocation);
                return;
            }
        }
    }

    private Vector2Int GetTargetDirection(Vector2Int _currentLocation, Vector2Int _targetLocation) {
        Vector2Int _targetDirection = _targetLocation - _currentLocation;
        if (_currentLocation.y % 2 == 0 && _targetDirection.y != 0) _targetDirection.x += 1;
        return _targetDirection;
    }
    
    private void MoveInRandomDirectionOnCurrentTile(PathfindingNode _startNode) {
        while (true) {
            Vector2Int _targetDirection = directions[Random.Range(0, directions.Length)];
            if (_startNode.PathfindingNodes[_targetDirection] != null) {
                MoveToTargetNode(_startNode.PathfindingNodes[_targetDirection]);
                return;
            }
        }
    }

    public void SetLocation(Vector2Int _location, int _nodeIndex) {
        currentLocation = _location;
        targetLocation = _location;

        currentNode = TileManagement.instance.GetTileAtLocation(currentLocation).TileObject.GetComponent<GameplayTile>().TilePathfinding.NodesOnTile[_nodeIndex];
        targetNode = currentNode;

        transform.position = currentNode.transform.position + new Vector3(Random.Range(-currentNode.Radius, currentNode.Radius), 0, Random.Range(-currentNode.Radius, currentNode.Radius));

        startPos = transform.position;
        targetPos = transform.position;

        path = null;
        targetDirection = Vector2Int.zero;
        finishedMoving = true;
    }
}
