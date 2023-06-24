using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Based on this: https://github.com/folospace/UnityPathFinding2D
// I love looking at random Github Repos, beautiful thing, very libertarian and anarchistic
// Must remember to credit the original author
public class PathfindingManager : MonoBehaviour {
    #region Variables

    public static PathfindingManager instance;

    [SerializeField] private List<int> walkableTileIds;

    Dictionary<Vector2Int, int> map = new Dictionary<Vector2Int, int>();
    Dictionary<Vector2Int, PathfindingLocation> pathfindingLocationsMap = new Dictionary<Vector2Int, PathfindingLocation>();

    public List<int> WalkableTileIds { get => walkableTileIds; set => walkableTileIds = value; }
    public Dictionary<Vector2Int, PathfindingLocation> PathfindingLocationsMap { get => pathfindingLocationsMap; set => pathfindingLocationsMap = value; }

    #endregion

    #region Core

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

    #endregion

    #region Public Functions

    /** find a path in hexagonal grid tilemaps (when grid rows are staggered with each other) **/
    public static List<Vector2Int> FindPath(Vector2Int from, Vector2Int to) {
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

    public static Vector2Int TileLocationToPathfindingLocation(Vector2Int _tileLoc) {
        int x = _tileLoc.x * 3;
        if (_tileLoc.x % 2 == 0) x += 1;

        int y = 2 + (_tileLoc.y * 3);

        return new Vector2Int(x, y);
    }

    public static Vector2Int GeneratePathfindingLocation(Vector2Int _localLoc, Vector2Int _tileLoc) {
        Vector2Int pathLoc = _localLoc;

        pathLoc.x += _tileLoc.x * 3;
        if (_tileLoc.y % 2 == 1) {
            if (_localLoc.y % 2 == 0) pathLoc.x += 2;
            else pathLoc.x += 1;
        }

        pathLoc.y += _tileLoc.y * 3;

        return pathLoc;
    }

    public static Vector2Int AddPathfindingLocationToMap(Vector2Int _localLoc, Vector2Int _tileLoc, int _tileID, PathfindingLocation _pathfindingLocation) {
        Vector2Int loc = RBHKLocationToPathfindingLocation(GeneratePathfindingLocation(_localLoc, _tileLoc));

        if (instance.map.ContainsKey(loc)) RemovePathfindingLocationFromMap(loc);
        instance.map.Add(loc, _tileID);
        instance.pathfindingLocationsMap.Add(loc, _pathfindingLocation);

        return loc;
    }

    public static void RemovePathfindingLocationFromMap(Vector2Int _pathfindingLocation) {
        instance.map.Remove(_pathfindingLocation);
        instance.pathfindingLocationsMap.Remove(_pathfindingLocation);
    }

    public static Vector2Int PathfindingLocationToRBHKLocation(Vector2Int _input) {
        Vector2Int output = new Vector2Int(_input.x, _input.y);
        if (output.y % 2 == 0) {
            output.x /= 2;
        } else {
            if (output.x > 1) {
                if (output.x % 2 != 0) output.x += 1;
                output.x /= 2;
            } else {
                output.x -= 1;
            }
        }
        return output;
    }

    public static Vector2Int RBHKLocationToPathfindingLocation(Vector2Int _input) {
        Vector2Int output = new Vector2Int(_input.x, _input.y);
        if (output.y % 2 == 0) {
            output.x *= 2;
        } else {
            output.x = -1 + (output.x * 2);
        }
        return output;
    }

    #endregion

    #region Pathfinding

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

    #endregion
}
