using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SaveLoad
{
    public static string SAVEPREFIX = Application.persistentDataPath;
    public const char BOOKPREFIX = '\\';
    public const string SUFFIX = ".budzogan";
    public static bool CheckFile(string path)
    {
        if (File.Exists(path))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
