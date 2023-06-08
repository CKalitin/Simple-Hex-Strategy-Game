using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceUIScaler : MonoBehaviour {
    [Tooltip("Leave null to use this Game Object.")]
    [SerializeField] private GameObject target;
    [Space]
    [SerializeField] private float uiScale = 0.05f;
    [SerializeField] private float maxScale = 999999f;

    private void Awake() {
        if (target == null) target = gameObject;
    }

    private void LateUpdate() {
        if (target.activeSelf) {
            float dist = Vector2.Distance(new Vector2(target.transform.position.y, target.transform.position.z), new Vector2(Camera.main.transform.position.y, Camera.main.transform.position.z));
            target.transform.localScale = new Vector3(Mathf.Clamp(dist * uiScale, 0, maxScale), Mathf.Clamp(dist * uiScale, 0, maxScale), Mathf.Clamp(dist * uiScale, 0, maxScale));
        }
    }
}
