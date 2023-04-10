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
            Debug.Log($"Unit Manager instance already exists on ({gameObject}), destroying this.");
            Destroy(this);
        }
    }

    private void OnEnable() {
        USNL.CallbackEvents.OnStructureActionPacket += OnStructureActionPacket;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnStructureActionPacket -= OnStructureActionPacket;
    }

    #endregion

    #region Structure Management

    public void AddGameplayStructure(Vector2Int _location, GameplayStructure _gameplayStructure) {
        if (gameplayStructures.ContainsKey(_location)) gameplayStructures[_location].Add(_gameplayStructure);
        else gameplayStructures.Add(_location, new List<GameplayStructure>() { _gameplayStructure });
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
            gameplayStructures[Vector2Int.RoundToInt(packet.TargetTileLocation)].ForEach(x => x.OnStructureActionPacket(packet.FromClient, packet.ActionID, packet.ConfigurationInts));
    }

    #endregion
}
