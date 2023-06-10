using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingText : MonoBehaviour {
    [SerializeField] private float delay;
    [SerializeField] private string[] texts;
    [SerializeField] private TMPro.TextMeshProUGUI text;

    private int index;

    private void Awake() {
        text.text = texts[index];

        StartCoroutine(TextCoroutine());
    }
    
    private IEnumerator TextCoroutine() {
        while (true) {
            yield return new WaitForSecondsRealtime(delay);
            
            index++;
            if (index >= texts.Length) yield break;
            text.text = texts[index];
        }
    }
}
