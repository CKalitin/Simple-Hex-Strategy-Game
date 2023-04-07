using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TilePathfinding : MonoBehaviour {
    [Tooltip("Start from 1,1 and go clockwise. The first node is the one on the top right, the second one is the one on the top left, etc. Goddamn Copilot explained better than I could.")]
    [SerializeField] private PathfindingNode[] nodes = new PathfindingNode[6];
    [Space]
    [Tooltip("This is an array of all the nodes on this tile. Automatically set by the code")]
    [SerializeField] private List<PathfindingNode> nodesOnTile;

    // nodes around the tile in every direction
    //    (0, 1)  (1, 1)
    // (-1,0) (self) (1, 0)
    //    (0,-1)  (1,-1)
    private Dictionary<Vector2Int, PathfindingNode> pathfindingNodes;

    public PathfindingNode[] NodesArray { get => nodes; set => nodes = value; }
    public Dictionary<Vector2Int, PathfindingNode> PathfindingNodes { get => pathfindingNodes; set => pathfindingNodes = value; }
    public List<PathfindingNode> NodesOnTile { get => nodesOnTile; set => nodesOnTile = value; }

    private void Awake() {
        pathfindingNodes = new Dictionary<Vector2Int, PathfindingNode>() { { new Vector2Int(1, 1), null }, { new Vector2Int(1, 0), null }, { new Vector2Int(1, -1), null }, { new Vector2Int(0, -1), null }, { new Vector2Int(-1, 0), null }, { new Vector2Int(0, 1), null } };
        for (int i = 0; i < pathfindingNodes.Keys.Count; i++) {
            pathfindingNodes[pathfindingNodes.Keys.ElementAt(i)] = nodes[i];
        }

        for (int i = 0; i < transform.childCount; i++) {
            nodesOnTile.Add(transform.GetChild(i).GetComponent<PathfindingNode>());
        }
    }
}
