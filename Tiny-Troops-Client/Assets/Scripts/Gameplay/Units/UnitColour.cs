using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitColour : MonoBehaviour {
    [SerializeField] private Material[] materials;
    [Space]
    [SerializeField] private MeshRenderer meshRenderer;

    private void Start() {
        meshRenderer.material = materials[MatchManager.instance.PlayerID];
    }
}
