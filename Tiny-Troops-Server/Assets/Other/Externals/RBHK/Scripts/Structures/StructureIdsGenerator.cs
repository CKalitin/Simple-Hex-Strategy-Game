using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "StructureIds", menuName = "RBHK/Structures/Structure Ids", order = 0)]
public class StructureIdsGenerator : ScriptableObject {
    [SerializeField] private string[] structureIds;
    [Space]
    [SerializeField] private string tileEnumPath = "Assets/Other/Enums/";
    [SerializeField] private string tileEnumName = "StructureID";

    public string[] StructureIdsArray { get => structureIds; set => structureIds = value; }
    public string ResourceEnumPath { get => tileEnumPath; set => tileEnumPath = value; }
    public string ResourceEnumName { get => tileEnumName; set => tileEnumName = value; }

    // This is used in the custom inspector / editor script
    public void UpdateEnums() {
        string filePathAndName = tileEnumPath + "StructureIds" + ".cs"; //The folder where the enum script is expected to exist

        using (StreamWriter streamWriter = new StreamWriter(filePathAndName)) {
            // Put the damned bracket on the right line
            streamWriter.WriteLine("public enum " + tileEnumName + " { ");
            for (int i = 0; i < structureIds.Length; i++) {
                streamWriter.WriteLine("\t" + structureIds[i] + ",");
            }
            streamWriter.WriteLine("}");
        }
    }
}
