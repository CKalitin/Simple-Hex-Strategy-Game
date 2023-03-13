using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "TileIds", menuName = "RBHK/Tiles/TileIds", order = 0)]
public class TileIds : ScriptableObject {
    [SerializeField] private string[] tileIds;
    [Space]
    [SerializeField] private string tileEnumPath = "Assets/Other/Enums/";
    [SerializeField] private string tileEnumName = "Tiles";

    public string[] ResourceIdsArray { get => tileIds; set => tileIds = value; }
    public string ResourceEnumPath { get => tileEnumPath; set => tileEnumPath = value; }
    public string ResourceEnumName { get => tileEnumName; set => tileEnumName = value; }

    // This is used in the custom inspector / editor script
    public void UpdateEnums() {
        string filePathAndName = tileEnumPath + "RBHKTileEnums" + ".cs"; //The folder where the enum script is expected to exist

        using (StreamWriter streamWriter = new StreamWriter(filePathAndName)) {
            // Put the damned bracket on the right line
            streamWriter.WriteLine("public enum " + tileEnumName + " { ");
            // Add line for Null tile
            streamWriter.WriteLine("\t" + "Null" + ",");
            for (int i = 0; i < tileIds.Length; i++) {
                streamWriter.WriteLine("\t" + tileIds[i] + ",");
            }
            streamWriter.WriteLine("}");
        }
    }
}
