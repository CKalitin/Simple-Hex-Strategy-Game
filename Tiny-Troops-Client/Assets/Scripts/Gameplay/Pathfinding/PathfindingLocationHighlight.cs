using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingLocationHighlight : MonoBehaviour {
    [SerializeField] private GameObject highlight;

    public GameObject Highlight { get => highlight; set => highlight = value; }
}
