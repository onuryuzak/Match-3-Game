using UnityEditor;
using UnityEngine;


public class DeletePlayerPrefs
{
    [MenuItem("Tools/Match-3 Management/Delete PlayerPrefs", false, 1)]
    public static void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
}