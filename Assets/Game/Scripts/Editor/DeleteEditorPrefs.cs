using UnityEditor;


public class DeleteEditorPrefs
{
    [MenuItem("Tools/Match-3 Management/Delete EditorPrefs", false, 2)]
    public static void DeleteAllEditorPrefs()
    {
        EditorPrefs.DeleteAll();
    }
}