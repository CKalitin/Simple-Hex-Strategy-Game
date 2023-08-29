using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureManager : MonoBehaviour {
    #region Variables

    public static StructureManager instance;

    private Dictionary<Vector2Int, List<GameplayStructure>> gameplayStructures = new Dictionary<Vector2Int, List<GameplayStructure>>();

    public Dictionary<Vector2Int, List<GameplayStructure>> GameplayStructures { get => gameplayStructures; set => gameplayStructures = value; }

    #endregion

    #region Core

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (instance == null) instance = this;
        else {
            Debug.Log($"Unit Manager instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }
    
    private void OnEnable() {
        USNL.CallbackEvents.OnStructureActionPacket += OnStructureActionPacket;
        USNL.CallbackEvents.OnStructureHealthPacket += OnStructureHealthPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnStructureActionPacket -= OnStructureActionPacket;
        USNL.CallbackEvents.OnStructureHealthPacket -= OnStructureHealthPacket;
    }

    #endregion

    #region Structure Management

    public void AddGameplayStructure(Vector2Int _location, GameplayStructure _gameplayStructure) {
        if (gameplayStructures.ContainsKey(_location))
            gameplayStructures[_location].Add(_gameplayStructure);
        else
            gameplayStructures.Add(_location, new List<GameplayStructure>() { _gameplayStructure });
    }

    public void RemoveGameplayStructure(Vector2Int _location, GameplayStructure _gameplayStructure) {
        if (gameplayStructures[_location].Count > 1)
            gameplayStructures[_location].Remove(_gameplayStructure);
        else
            gameplayStructures.Remove(_location);
    }

    private void OnStructureActionPacket(object _packetObject) {
        USNL.StructureActionPacket packet = (USNL.StructureActionPacket)_packetObject;
        
        if (gameplayStructures.ContainsKey(Vector2Int.RoundToInt(packet.TargetTileLocation)))
            gameplayStructures[Vector2Int.RoundToInt(packet.TargetTileLocation)].ForEach(x => x.OnStructureActionPacket(packet.PlayerID, packet.ActionID, packet.ConfigurationInts));
    }

    private void OnStructureHealthPacket(object _packetObject) {
        USNL.StructureHealthPacket packet = (USNL.StructureHealthPacket)_packetObject;
        if (gameplayStructures.ContainsKey(Vector2Int.RoundToInt(packet.Location))) {
            gameplayStructures[Vector2Int.RoundToInt(packet.Location)].ForEach(x => x.OnStructureHealthPacket(packet.Health, packet.MaxHealth));
        }
    }

    #endregion
}
