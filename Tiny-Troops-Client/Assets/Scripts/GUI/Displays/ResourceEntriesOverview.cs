using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceEntriesOverview : MonoBehaviour {
    private void Update() {
        // THE CLIENT DOESNT HAVE RESOURCE ENTRIES!!!?!!!!!

        string output = "";
        
        for (int i = 0; i < ResourceManager.instances[0].ResourceEntries.Values.Count; i++) {
            output += "(" + ResourceManager.instances[0].ResourceEntries.Values[i].ResourceId + ", " + ResourceManager.instances[0].ResourceEntries.Values[i].Change + "), ";
        }

        if (output.Length <= 2) return;
        
        output = output.Substring(0, output.Length - 2);
        Debug.Log(output);
    }
}
