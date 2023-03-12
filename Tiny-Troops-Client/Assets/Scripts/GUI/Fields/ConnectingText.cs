using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingText : MonoBehaviour {
    [SerializeField] private float delay;
    [SerializeField] private string[] texts;
    [SerializeField] private TMPro.TextMeshProUGUI text;

    private int index;
    private float timer;

    private void Start() {
        text.text = texts[index];
    }

    private void Update() {
        timer += Time.deltaTime;
        if (timer >= delay) {
            timer = 0;
            index++;
            if (index >= texts.Length) return;
            text.text = texts[index];
        }
    }
}
