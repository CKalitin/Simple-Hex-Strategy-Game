using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureManager : MonoBehaviour {
    #region Variables

    public static StructureManager instance;

    [Header("Health Regeneration")]
    [Tooltip("In Seconds")]
    [SerializeField] private float healthRegenTickTime = 2f;
    [SerializeField] private float healthRegenAmount = 2f;

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
        MatchManager.OnMatchStateChanged += OnMatchStateChanged;
    }

    private void OnDisable() {
        USNL.CallbackEvents.OnStructureActionPacket -= OnStructureActionPacket;
        MatchManager.OnMatchStateChanged -= OnMatchStateChanged;
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

    #region Health Regen

    private IEnumerator HealthRegenCoroutine() {
        while (true) {
            yield return new WaitForSeconds(healthRegenTickTime);
            foreach (List<GameplayStructure> gs in gameplayStructures.Values) {
                for (int i = 0; i < gs.Count; i++) {
                    if (gs[i].GetComponent<ConstructionStructure>()) continue;
                    if (gs[i].BeingDestroyed) continue;
                    Health h;
                    if ((h = gs[i].GetComponent<Health>()) != null) {
                        h.ChangeHealth(healthRegenAmount);
                        if (h.CurrentHealth > h.MaxHealth) h.SetHealth(h.MaxHealth);
                    }
                }
            }
        }
    }

    private void OnMatchStateChanged(MatchState _ms) {
        if (_ms == MatchState.InGame) StartCoroutine(HealthRegenCoroutine());
        else StopAllCoroutines();
    }

    #endregion
}
