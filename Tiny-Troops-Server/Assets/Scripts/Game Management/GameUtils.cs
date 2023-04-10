using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtils {
    public static int Random(int _seed, int _min, int _max) {
        return new System.Random(_seed).Next(_min, _max);
    }
    public static float Random(int _seed, float _min, float _max) {
        return new System.Random(_seed).Next(Mathf.RoundToInt(_min * 1000000) / 1000000, Mathf.RoundToInt(_max * 1000000) / 1000000);
    }
}
