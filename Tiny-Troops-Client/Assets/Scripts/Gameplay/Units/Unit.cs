using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackPriority {
    Units,
    Structures
}

// Code to control units is in Tile Selector and Unit Selector
public class Unit : MonoBehaviour {
    #region Variables

    [Header("Attack")]
    [SerializeField] private float unitAttackDamage;
    [SerializeField] private float structureAttackDamage;
    [SerializeField] private AttackPriority attackPriority;
    [Space]
    [SerializeField] private float trainingTime = 5f;
    [SerializeField] private string unitDisplayName;
    [SerializeField] private RBHKCost[] unitCost;

    [Header("References")]
    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private Health health;
    
    [Header("Debug")]
    [Tooltip("Shown to dev only for debug purposes.")]
    [SerializeField] private int playerID;
    [Tooltip("Shown to dev only for debug purposes.")]
    [SerializeField] private int randomSeed;

    private int unitUUID;

    private bool attacking = false;

    private PathfindingAgent pathfindingAgent;
    
    public float UnitAttackDamage { get => unitAttackDamage; set => unitAttackDamage = value; }
    public float StructureAttackDamage { get => structureAttackDamage; set => structureAttackDamage = value; }
    public AttackPriority AttackPriority { get => attackPriority; set => attackPriority = value; }
    public float TrainingTime { get => trainingTime; set => trainingTime = value; }
    public string UnitDisplayName { get => unitDisplayName; set => unitDisplayName = value; }
    public RBHKCost[] UnitCost { get => unitCost; set => unitCost = value; }

    public Health Health { get => health; set => health = value; }
    public int PlayerID { get => playerID; set => playerID = value; }
    public int RandomSeed { get => randomSeed; set => randomSeed = value; }
    public int UnitUUID { get => unitUUID; set => unitUUID = value; }
    public bool Attacking { get => attacking; set => attacking = value; }
    public PathfindingAgent PathfindingAgent { get => pathfindingAgent; set => pathfindingAgent = value; }

    public Vector2Int Location { get => pathfindingAgent.CurrentTile; }
    public bool Selected { get => selectedIndicator.activeSelf; }

    #endregion

    #region Core

    private void Awake() {
        pathfindingAgent = GetComponent<PathfindingAgent>();
        
        if (selectedIndicator) selectedIndicator.SetActive(false);
    }

    private void Start() {
        UnitManager.instance.AddUnit(UnitUUID, gameObject, this, playerID, randomSeed);
    }

    private void OnDestroy() {
        if (UnitSelector.instance.SelectedUnits.ContainsKey(unitUUID)) TileSelector.instance.DeselectUnit(unitUUID);
        UnitManager.instance.RemoveUnit(UnitUUID);
    }

    #endregion

    #region Unit

    public void ToggleSelectedIndicator(bool _toggle) {
        try {
            selectedIndicator.SetActive(_toggle);
        } catch(Exception _ex) {
            Debug.Log("Couldn't toggle selected indicator Error\n" + _ex);
        }
    }

    #endregion
}
