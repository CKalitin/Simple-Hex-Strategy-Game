using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtils {
    public static int Random(int _seed, int _min, int _max) {
        return new System.Random(_seed).Next(_min, _max);
    }

    public static float Random(int _seed, float _min, float _max) {
        return new System.Random(_seed).Next(Mathf.RoundToInt(_min * 1000000), Mathf.RoundToInt(_max * 1000000)) / 1000000f;
    }

    public static int Random(ref System.Random _random, int _min, int _max) {
        return _random.Next(_min, _max);
    }

    public static float Random(ref System.Random _random, float _min, float _max) {
        return _random.Next(Mathf.RoundToInt(_min * 1000000), Mathf.RoundToInt(_max * 1000000)) / 1000000f;
    }

    public static Tile GetTileParent(Transform _transform) {
        Tile output = null;
        Transform t = _transform.parent;
        while (true) {
            if (t.GetComponent<Tile>()) {
                // If tile script found
                output = t.GetComponent<Tile>();
                break;
            } else {
                // If we're at the top of the heirarchy, break out of the loop.
                if (t.parent == null) break;
                t = t.parent;
            }
        }
        return output;
    }
}
