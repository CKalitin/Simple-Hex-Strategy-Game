using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour {
    [SerializeField] private int playerID;

    public int PlayerID { get => playerID; set => playerID = value; }
}
