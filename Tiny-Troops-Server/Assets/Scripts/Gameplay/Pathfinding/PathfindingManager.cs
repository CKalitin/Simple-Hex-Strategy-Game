using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Based on this: https://github.com/folospace/UnityPathFinding2D
// I love looking at random Github Repos, beautiful thing, very libertarian and anarchistic
// Must remember to credit the original author
public class PathfindingManager : MonoBehaviour {
    public static PathfindingManager instance;

    [SerializeField] private List<int> walkableTileIds;

    Dictionary<Vector2Int, int> map = null;

    public List<int> WalkableTileIds { get => walkableTileIds; set => walkableTileIds = value; }

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (FindObjectsOfType<PathfindingManager>().Length > 1) {
            Destroy(this);
        } else {
            instance = this;
        }
    }

    // Convert from my tile system to the pathfinding system
    private static void TileMapToPathfindingMap() {
        instance.map = new Dictionary<Vector2Int, int>();
        Dictionary<Vector2Int, int> tempMap = TileManagement.instance.GetTiles.ToDictionary(x => x.Key, x => (int)x.Value.TileId);

        foreach (var tile in tempMap) {
            instance.map.Add(RBHKLocationToPathfindingLocation(tile.Key), tile.Value);
        }
    }

    private static Vector2Int PathfindingLocationToRBHKLocation(Vector2Int _input) {
        Vector2Int output = _input;
        if (output.y % 2 == 0) {
            output.x /= 2;
        } else {
            if (output.x > 1) {
                if (output.x % 2 != 0) output.x -= 1;
                output.x /= 2;
            } else {
                output.x -= 1;
            }
        }
        return output;
    }

    private static Vector2Int RBHKLocationToPathfindingLocation(Vector2Int _input) {
        Vector2Int output = _input;
        if (output.y % 2 == 0) {
            output.x *= 2;
        } else {
            output.x = 1 + (output.x * 2);
        }
        return output;
    }

    /** find a path in hexagonal grid tilemaps (when grid rows are staggered with each other) **/
    public static List<Vector2Int> FindPath(Vector2Int from, Vector2Int to) {
        if (instance.map == null) TileMapToPathfindingMap();

        from = RBHKLocationToPathfindingLocation(from);
        to = RBHKLocationToPathfindingLocation(to);

        Func<Vector2Int, Vector2Int, float> getDistance = delegate (Vector2Int a, Vector2Int b) {
            float xDistance = Mathf.Abs(a.x - b.x);
            float yDistance = Mathf.Abs(a.y - b.y) * Mathf.Sqrt(3);
            return xDistance * xDistance + yDistance * yDistance;
        };
        Func<Vector2Int, List<Vector2Int>> getNeighbors = delegate (Vector2Int pos) {
            var neighbors = new List<Vector2Int>();
            neighbors.Add(new Vector2Int(pos.x + 1, pos.y + 1));
            neighbors.Add(new Vector2Int(pos.x - 1, pos.y + 1));
            neighbors.Add(new Vector2Int(pos.x + 1, pos.y - 1));
            neighbors.Add(new Vector2Int(pos.x - 1, pos.y - 1));
            neighbors.Add(new Vector2Int(pos.x - 2, pos.y));
            neighbors.Add(new Vector2Int(pos.x + 2, pos.y));
            return neighbors;
        };
        return astar(from, to, instance.map, instance.walkableTileIds, getDistance, getNeighbors);
    }

    private static List<Vector2Int> astar(Vector2Int from, Vector2Int to, Dictionary<Vector2Int, int> map, List<int> passableValues,
                      Func<Vector2Int, Vector2Int, float> getDistance, Func<Vector2Int, List<Vector2Int>> getNeighbors) {
        var result = new List<Vector2Int>();
        if (from == to) {
            result.Add(from);
            return result;
        }
        Node finalNode;
        List<Node> open = new List<Node>();
        if (findDest(new Node(null, from, getDistance(from, to), 0), open, map, to, out finalNode, passableValues, getDistance, getNeighbors)) {
            while (finalNode != null) {
                result.Add(finalNode.pos);
                finalNode = finalNode.preNode;
            }
        }
        result.Reverse();

        List<Vector2Int> finalResult = new List<Vector2Int>();
        for (int i = 0; i < result.Count; i++) {
            finalResult.Add(PathfindingLocationToRBHKLocation(result[i]));
        }

        return finalResult;
    }

    private static bool findDest(Node currentNode, List<Node> openList,
                         Dictionary<Vector2Int, int> map, Vector2Int to, out Node finalNode, List<int> passableValues,
                      Func<Vector2Int, Vector2Int, float> getDistance, Func<Vector2Int, List<Vector2Int>> getNeighbors) {
        if (currentNode == null) {
            finalNode = null;
            return false;
        } else if (currentNode.pos == to) {
            finalNode = currentNode;
            return true;
        }
        currentNode.open = false;
        openList.Add(currentNode);

        foreach (var item in getNeighbors(currentNode.pos)) {
            if (map.ContainsKey(item) && passableValues.Contains(map[item])) {
                findTemp(openList, currentNode, item, to, getDistance);
            }
        }
        var next = openList.FindAll(obj => obj.open).Min();
        return findDest(next, openList, map, to, out finalNode, passableValues, getDistance, getNeighbors);
    }

    private static void findTemp(List<Node> openList, Node currentNode, Vector2Int from, Vector2Int to, Func<Vector2Int, Vector2Int, float> getDistance) {

        Node temp = openList.Find(obj => obj.pos == (from));
        if (temp == null) {
            temp = new Node(currentNode, from, getDistance(from, to), currentNode.gScore + 1);
            openList.Add(temp);
        } else if (temp.open && temp.gScore > currentNode.gScore + 1) {
            temp.gScore = currentNode.gScore + 1;
            temp.fScore = temp.hScore + temp.gScore;
            temp.preNode = currentNode;
        }
    }

    class Node : IComparable {
        public Node preNode;
        public Vector2Int pos;
        public float fScore;
        public float hScore;
        public float gScore;
        public bool open = true;

        public Node(Node prePos, Vector2Int pos, float hScore, float gScore) {
            this.preNode = prePos;
            this.pos = pos;
            this.hScore = hScore;
            this.gScore = gScore;
            this.fScore = hScore + gScore;
        }

        public int CompareTo(object obj) {
            Node temp = obj as Node;

            if (temp == null) return 1;

            if (Mathf.Abs(this.fScore - temp.fScore) < 0.01f) {
                return this.fScore > temp.fScore ? 1 : -1;
            }

            if (Mathf.Abs(this.hScore - temp.hScore) < 0.01f) {
                return this.hScore > temp.hScore ? 1 : -1;
            }
            return 0;
        }
    }
}
