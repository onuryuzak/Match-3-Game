using UnityEngine;
using UnityEngine.Assertions;
using FullSerializer;


public static class FileUtils
{
    public static T LoadJsonFile<T>(fsSerializer serializer, string path) where T : class
    {
        var textAsset = Resources.Load<TextAsset>(path);
        Assert.IsNotNull((textAsset));
        var data = fsJsonParser.Parse(textAsset.text);
        object deserialized = null;
        serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();
        return deserialized as T;
    }

    public static bool FileExists(string path)
    {
        var textAsset = Resources.Load<TextAsset>(path);
        return textAsset != null;
    }
}