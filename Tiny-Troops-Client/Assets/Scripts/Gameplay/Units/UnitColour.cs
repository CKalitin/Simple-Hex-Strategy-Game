using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitColour : MonoBehaviour {
    [SerializeField] private MeshRenderer meshRenderer;
    [Space]
    [SerializeField] private Unit unit;

    private void Start() {
        int index = ColorManager.instance.ColorToIndex(PlayerInfoManager.instance.PlayerInfos[unit.PlayerID].Color);
        meshRenderer.material = ColorManager.instance.ColorMaterials[index];
    }
}
