using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName ="ResourceIds", menuName = "RBHK/Resources/ResourceIds", order =0)]
public class ResourceIds : ScriptableObject {
    [Header("Resources Ids & Resource Entry Ids")]
    [SerializeField] private string[] resourceIds;
    [Space]
    [SerializeField] private string[] resourceEntryIds;

    [Header("Enum Stuff")]
    [SerializeField] private string resourceEnumPath = "Assets/Other/Enums/";
    [Space]
    [SerializeField] private string resourceEnumName = "Resources";
    [SerializeField] private string resourceEntryEnumName = "ResourceEntries";

    public string ResourceEnumPath { get => resourceEnumPath; set => resourceEnumPath = value; }
    public string ResourceEnumName { get => resourceEnumName; set => resourceEnumName = value; }
    public string ResourceEntryEnumName { get => resourceEntryEnumName; set => resourceEntryEnumName = value; }
    public string[] ResourceIdsArray { get => resourceIds; set => resourceIds = value; }
    public string[] ResourceEntryIdsArray { get => resourceEntryIds; set => resourceEntryIds = value; }

    // This is used in the custom inspector / editor script
    public void UpdateEnums() {
        string filePathAndName = resourceEnumPath + "RBHKResourceEnums" + ".cs"; //The folder where the enum script is expected to exist

        using (StreamWriter streamWriter = new StreamWriter(filePathAndName)) {
            // Put the damned bracket on the right line
            streamWriter.WriteLine("public enum " + resourceEnumName + " { ");
            for (int i = 0; i < resourceIds.Length; i++) {
                streamWriter.WriteLine("\t" + resourceIds[i] + ",");
            }
            streamWriter.WriteLine("}");

            // Write a blank line between the enums
            streamWriter.WriteLine("");

            // Put the damned bracket on the right line
            streamWriter.WriteLine("public enum " + resourceEntryEnumName + " { ");
            for (int i = 0; i < resourceEntryIds.Length; i++) {
                streamWriter.WriteLine("\t" + resourceEntryIds[i] + ",");
            }
            streamWriter.WriteLine("}");
        }
    }
}
