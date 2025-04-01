using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static void WriteFile(string content, string path)
    {
        string filePath = Path.Combine(Application.persistentDataPath, path);
        
        try
        {
            File.WriteAllText(filePath, content);
            Debug.Log("Written to: " + filePath);
        }
        catch (IOException e)
        {
            Debug.LogError("Failed to write file: " + e.Message);
        }
    }
}
