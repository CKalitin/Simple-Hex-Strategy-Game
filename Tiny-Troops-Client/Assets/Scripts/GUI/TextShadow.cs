using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextShadow : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI sourceText;
    [SerializeField] private TextMeshProUGUI text;

    private void LateUpdate() {
        text.text = sourceText.text;
    }
}
