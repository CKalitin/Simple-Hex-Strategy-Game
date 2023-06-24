using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStructure : MonoBehaviour {
    #region Variables
    
    [SerializeField] private Health health;

    private Vector2Int tileLocation;
    
    private bool addedToStructureManager = false;

    private float previousHealth = -1f; // This is -1 so on the first Update() it gets updated

    public delegate void StructureActionCallback(int playerID, int actionID, int[] configurationInts);
    public event StructureActionCallback OnStructureAction;
    
    public Vector2Int TileLocation { get => tileLocation; set => tileLocation = value; }

    #endregion

    #region Core

    private void Start() {
        if (addedToStructureManager == false && GetComponent<Structure>().Tile.TileInfo != null) {
            tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
            StructureManager.instance.AddGameplayStructure(tileLocation, this);
            addedToStructureManager = true;
        }
    }

    private void Update() {
        if (previousHealth != health.CurrentHealth) {
            previousHealth = health.CurrentHealth;

            USNL.PacketSend.StructureHealth(TileLocation, health.CurrentHealth, health.MaxHealth);
            if (health.CurrentHealth <= 0) GetComponent<Structure>().DestroyStructure();
        }
    }

    private void OnEnable() {
        if (addedToStructureManager == false && GetComponent<Structure>().Tile.TileInfo != null) {
            tileLocation = GetComponent<Structure>().Tile.TileInfo.Location;
            StructureManager.instance.AddGameplayStructure(tileLocation, this);
            addedToStructureManager = true;
        }
    }

    private void OnDisable() {
        StructureManager.instance.RemoveGameplayStructure(tileLocation, this);
        addedToStructureManager = false;
    }

    #endregion

    #region Structure Actions

    public void OnStructureActionPacket(int _playerID, int _actionID, int[] _configurationInts) {
        OnStructureAction(_playerID, _actionID, _configurationInts);
    }

    #endregion
}
