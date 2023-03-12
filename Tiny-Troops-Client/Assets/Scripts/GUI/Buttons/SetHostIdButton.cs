using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetHostIdButton : MonoBehaviour {
    [SerializeField] private TMP_InputField hostIdField;
    [Space]
    [SerializeField] private string id;
    
    public void SetIdField() {
        hostIdField.text = id;
    }
}
