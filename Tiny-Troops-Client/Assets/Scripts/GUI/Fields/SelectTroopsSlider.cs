using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SelectTroopsSlider : MonoBehaviour {
    public static SelectTroopsSlider instance;

    [SerializeField] private Slider troopsSlider;
    [SerializeField] private TextMeshProUGUI selectedUnitsText;

    // Units that the player will control after the split
    private List<int> selectedUnits = new List<int>();

    private int seed;

    public List<int> SelectedUnits { get => selectedUnits; set => selectedUnits = value; }

    private void Awake() {
        seed = Random.Range(0, 999999999);

        Singleton();
    }

    private void Singleton() {
        if (instance == null) {
            instance = this;
        } else {
            Debug.Log($"Select Troops Slider instance already exists on ({gameObject}), destroying this.", gameObject);
            Destroy(this);
        }
    }

    private void Update() {
        troopsSlider.maxValue = UnitSelector.instance.SelectedUnits.Count;
        selectedUnitsText.text = selectedUnits.Count + "/" + UnitSelector.instance.SelectedUnits.Count;
    }

    public void SplitButtonPressed() {
        DeselectAllUnits();

        for (int i = 0; i < selectedUnits.Count; i++) {
            SelectUnit(selectedUnits[i]);
        }
    }

    public void SliderValueChanged() {
        if (troopsSlider.value < selectedUnits.Count) {
            selectedUnits.RemoveRange((int)troopsSlider.value, selectedUnits.Count - (int)troopsSlider.value);
        } else if (troopsSlider.value > selectedUnits.Count) {
            SelectMoreUnits((int)troopsSlider.value - selectedUnits.Count);
        }
    }

    private void SelectMoreUnits(int _more) {
        List<int> availableUnits = UnitSelector.instance.SelectedUnits.Select(x => x.Key).ToList();

        for (int i = 0; i < selectedUnits.Count; i++) availableUnits.Remove(selectedUnits[i]);

        // Use a random with a set seed so the player gets random units selected, but always in the same order
        System.Random random = new System.Random(seed);
        for (int i = 0; i < selectedUnits.Count; i++) random.Next(0, availableUnits.Count);

        for (int i = 0; i < _more; i++) {
            selectedUnits.Add(availableUnits[random.Next(0, availableUnits.Count)]);
            availableUnits.Remove(selectedUnits[selectedUnits.Count - 1]);
        }
    }
    private void SelectUnit(int _unitUUID) {
        if (UnitSelector.instance.SelectedUnits.ContainsKey(_unitUUID)) return;
        UnitSelector.instance.SelectedUnits.Add(_unitUUID, UnitManager.instance.Units[_unitUUID]);
        UnitSelector.instance.SelectedUnits[_unitUUID].Script.ToggleSelectedIndicator(true);
    }

    private void DeselectAllUnits() {
        foreach (KeyValuePair<int, UnitInfo> unit in UnitSelector.instance.SelectedUnits) {
            UnitSelector.instance.SelectedUnits[unit.Key].Script.ToggleSelectedIndicator(false);
        }
        UnitSelector.instance.SelectedUnits = new Dictionary<int, UnitInfo>();
    }

    public void UnitDeselected(int _unitUUID) {
        if (selectedUnits.Contains(_unitUUID)) {
            selectedUnits.Remove(_unitUUID);
            SelectMoreUnits(1);
        }
    }
}
