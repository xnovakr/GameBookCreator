using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class BookExporter
{
    public static char SEPARATOR = '~';
    public static string EXPORTSUFFIX = ".molk";
    public static void ExportBook(GameObject book)
    {
        string savePrefix = GameObject.Find("Library Manager").GetComponent<LibraryManager>().savingPath;
        string path = savePrefix + SaveLoad.BOOKPREFIX + book.transform.name;
        string bookName = path.Split('\\')[path.Split('\\').Length - 1];
        string textToSave = bookName;
        int pagesCount = CountPages(path);
        //Debug.Log(pagesCount + " " + path + " " + bookName);

        string[] savedFiles = Directory.GetFiles(path);
        foreach(string file in savedFiles)
        {
            string fileName = file.Split('\\')[file.Split('\\').Length-1];
            if (fileName.StartsWith("page"))
            {
                textToSave += SEPARATOR;
                textToSave += File.ReadAllText(file);
                string temp1 = fileName.Split('-')[1].Split('.')[0];
                Debug.Log(temp1);
                foreach (string file2 in savedFiles)
                {
                    string pathName = file2.Split('\\')[file2.Split('\\').Length - 1];
                    if (pathName.StartsWith("path"))
                    {
                        string temp2 = pathName.Split('-')[1].Split('.')[0];
                        if (temp1 == temp2)
                        {
                            textToSave += SEPARATOR;
                            textToSave += File.ReadAllText(file2);
                            Debug.Log(temp2);
                            break;
                        }
                    }
                }
            }
        }

        string skillsPath = path + SaveLoad.BOOKPREFIX + SaveLoadPageProperties.SKILLS + SaveLoad.SUFFIX;
        string artefactPath = path + SaveLoad.BOOKPREFIX + SaveLoadPageProperties.ARTEFACTS + SaveLoad.SUFFIX;
        string checkpointsPath = path + SaveLoad.BOOKPREFIX + CurrentPageManager.CHECKPOINTFILENAME;
        string endingsPath = path + SaveLoad.BOOKPREFIX + CurrentPageManager.ENDINGFILENAME;

        textToSave += SEPARATOR;
        if (File.Exists(checkpointsPath)) textToSave += File.ReadAllText(checkpointsPath) + '|';
        if (File.Exists(endingsPath)) textToSave += File.ReadAllText(endingsPath);
        textToSave += SEPARATOR;
        if (File.Exists(skillsPath)) textToSave += File.ReadAllText(skillsPath);
        textToSave += SEPARATOR;
        if (File.Exists(artefactPath)) textToSave += File.ReadAllText(artefactPath);

        Debug.Log(textToSave);
        string exportSavingPath = savePrefix + SaveLoad.BOOKPREFIX + book.transform.name + EXPORTSUFFIX;
        File.WriteAllText(exportSavingPath, textToSave);
    }
    public static int CountPages(string path)
    {
        string[] files = Directory.GetFiles(path, "page*");
        return files.Length;
    }
}
