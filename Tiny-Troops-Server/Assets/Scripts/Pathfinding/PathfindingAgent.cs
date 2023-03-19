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
    private bool reachedTargetLocation = true;
    private bool finishedMoving = true;
    private Vector2Int[] directions = new Vector2Int[6] { new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(0, 1) };

    private void Start() {
        PathfindToLocation(new Vector2Int(4, 4));
    }

    private void Update() {
        Move();
    }

    public void Initialize(Vector2Int _currentLocation, PathfindingNode _currentNode) {
        currentLocation = _currentLocation;
        currentNode = _currentNode;
        startPos = transform.position;
    }
    
    private void Move() {
        if (finishedMoving) return;
        if (targetNode == null || currentNode == null) {
            targetNode = TileManagement.instance.GetTileAtLocation(currentLocation).TileObject.GetComponent<GameplayTile>().TilePathfinding.PathfindingNodes[targetLocation - currentLocation];
            MoveToTargetNode(TileManagement.instance.GetTileAtLocation(currentLocation).TileObject.GetComponent<GameplayTile>().TilePathfinding.PathfindingNodes[targetLocation - currentLocation]);
        }
        
        Vector3 direction = (startPos - targetPos).normalized;
        direction.y = 0;
        transform.position = Vector3.MoveTowards(transform.position, direction, moveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, targetPos) <= 0.3f) {
            currentNode = targetNode;

            ReachedTargetNode();
        }
    }

    private void ReachedTargetNode() {
        if (reachedTargetLocation) {
            targetLocation = currentLocation;
            targetNode = null;
            targetPos = transform.position;
            finishedMoving = true;
            return;
        }

        if (currentLocation == targetLocation) {
            path = null;
            reachedTargetLocation = true;

            // Pick random node to move to
            while (true) {
                targetDirection = directions[Random.Range(0, directions.Length)];
                PathfindingNode node = currentNode.PathfindingNodes[targetDirection];
                if (node != null) break;
                MoveToTargetNode(node);
            }
            
            return;
        }

        // If next node is on another tile
        if (targetNode.FinalNode) {
            path.RemoveAt(0);
            targetLocation = path[1];
            MoveToTargetNode(TileManagement.instance.GetTileAtLocation(path[1]).TileObject.GetComponent<GameplayTile>().TilePathfinding.PathfindingNodes[currentLocation - targetLocation]);
            return;
        }

        MoveToTargetNode(currentNode.PathfindingNodes[targetDirection]);
    }

    private void MoveToTargetNode(PathfindingNode _targetNode) {
        currentLocation = targetLocation;
        currentNode = targetNode;
        startPos = transform.position;
        
        targetDirection = targetDirection - currentLocation;
        targetNode = _targetNode;
        targetPos = targetNode.transform.position + new Vector3(Random.Range(-targetNode.Radius, targetNode.Radius), 0, Random.Range(-targetNode.Radius, targetNode.Radius));
    }

    public void PathfindToLocation(Vector2Int _targetLocation) {
        path = PathfindingManager.FindPath(currentLocation, _targetLocation);

        if (path.Count <= 1) {
            path = null;
            reachedTargetLocation = true;

            // Pick random node to move to
            while (true) {
                targetDirection = directions[Random.Range(0, directions.Length)];
                PathfindingNode node = currentNode.PathfindingNodes[targetDirection];
                if (node != null) break;
                MoveToTargetNode(node);
            }

            return;
        }
        
        targetLocation = path[1];
        targetDirection = targetDirection - currentLocation;
        reachedTargetLocation = false;
        finishedMoving = false;

        targetNode = currentNode.PathfindingNodes[targetDirection];
        if (targetNode.FinalNode) {
            path.RemoveAt(0);
            targetLocation = path[1];
            MoveToTargetNode(TileManagement.instance.GetTileAtLocation(path[1]).TileObject.GetComponent<TilePathfinding>().PathfindingNodes[currentLocation - targetLocation]);
            return;
        }
    }
}
