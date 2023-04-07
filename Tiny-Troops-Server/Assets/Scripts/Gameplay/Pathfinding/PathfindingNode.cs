using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathfindingNode : MonoBehaviour {
    [Tooltip("Start from 1,1 and go clockwise. The first node is the one on the top right, the second one is the one on the top left, etc. Goddamn Copilot explained better than I could.")]
    [SerializeField] private PathfindingNode[] nodes = new PathfindingNode[6];
    [Space]
    [Tooltip("Final node on a tile, next node is on another tile.")]
    [SerializeField] private bool finalNode;
    [Space]
    [Tooltip("Radius around this node that agents can move to.\n So they don't all stay in the same exact position.")]
    [SerializeField] private float radius;

    // nodes around the tile in every direction
    //    (0, 1)  (1, 1)
    // (-1,0) (self) (1, 0)
    //    (0,-1)  (1,-1)
    private Dictionary<Vector2Int, PathfindingNode> pathfindingNodes;

    public Dictionary<Vector2Int, PathfindingNode> PathfindingNodes { get => pathfindingNodes; set => pathfindingNodes = value; }
    public PathfindingNode[] NodesArray { get => nodes; set => nodes = value; }
    public bool FinalNode { get => finalNode; set => finalNode = value; }
    public float Radius { get => radius; set => radius = value; }

    private void Awake() {
        pathfindingNodes = new Dictionary<Vector2Int, PathfindingNode>() { { new Vector2Int(1, 1), null }, { new Vector2Int(1, 0), null }, { new Vector2Int(1, -1), null }, { new Vector2Int(0, -1), null }, { new Vector2Int(-1, 0), null }, { new Vector2Int(0, 1), null } };
        for (int i = 0; i < pathfindingNodes.Keys.Count; i++) {
            pathfindingNodes[pathfindingNodes.Keys.ElementAt(i)] = nodes[i];
        }
    }
}
