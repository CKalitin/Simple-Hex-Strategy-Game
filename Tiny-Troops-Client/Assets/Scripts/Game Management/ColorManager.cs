using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour {
    public static ColorManager instance;

    [SerializeField] private Color[] colors;
    [SerializeField] private Material[] colorMaterials;

    public Color[] Colors { get => colors; set => colors = value; }
    public Material[] ColorMaterials { get => colorMaterials; set => colorMaterials = value; }

    private void Awake() {
        Singleton();
    }

    private void Singleton() {
        if (instance == null) instance = this;
        else {
            Debug.Log($"Color Manager instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    public int ColorToIndex(Color _color) {
        // Loop through colors and look for matches
        for (int i = 0; i < colors.Length; i++) {
            if (colors[i] == _color) return i;
        }
        return -1;
    }
}
