using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

public static class SaveLoadPage
{
    public const string FILENAME = "\\page-";
    
    // also need book names for saving each book into individual folder

    public static void SavePageText(string bookName, string pageIndex, string textToSave)
    {
        string savingPath = GameObject.Find("PageManager").GetComponent<CurrentPageManager>().savingPath;
        string path = savingPath + SaveLoad.BOOKPREFIX + bookName + FILENAME + pageIndex + SaveLoad.SUFFIX;

        if (!CheckFolder(bookName))
        {
            Debug.Log("Path not valid creating folder");
            CreateFolder(bookName);
        }// checks if save folder exist

        SavePageTextWithCustomPath(path, textToSave);
    }
    public static void SavePageTextWithCustomPath(string path, string textToSave)
    {
        File.WriteAllText(path, textToSave);
        //FileStream stream = new FileStream(path, FileMode.Create);// create save file
        //StreamWriter writer = new StreamWriter(stream);// open file for writing
        //writer.Write(textToSave);// save text into file
        //writer.Close();// close writing

        //stream.Close();// close file
    }
    public static string LoadPageText(string bookName, string pageIndex)
    {
        string savingPath = GameObject.Find("PageManager").GetComponent<CurrentPageManager>().savingPath;
        string path = savingPath + SaveLoad.BOOKPREFIX + bookName + FILENAME + pageIndex + SaveLoad.SUFFIX;

        if (!CheckFolder(bookName))
        {
            Debug.Log("Selected folder doesent exist!");
            Debug.Log(path);
            return null;
        }// checks if save folder exist
        else if (!CheckSave(bookName, pageIndex))
        {
            Debug.Log("Selected save file doesent exist!");
            return null;
        }// checks if save file exist
        
        //FileStream stream = new FileStream(path, FileMode.Open);// open save file
        //StreamReader reader = new StreamReader(stream);// open stream for reading
        //string textFromFile = reader.ReadToEnd();// save text from file to variable
        //reader.Close();// close reading stream
        //stream.Close();//close file
        return File.ReadAllText(path);
    }
    public static bool CheckSave(string bookName, string index)
    {
        string savingPath = GameObject.Find("PageManager").GetComponent<CurrentPageManager>().savingPath;
        string path = savingPath + SaveLoad.BOOKPREFIX + bookName + FILENAME + index + SaveLoad.SUFFIX;
        //Debug.Log(path);
        return SaveLoad.CheckFile(path);
    }
    public static void DeleteSave(string bookName, int saveIndex)
    {
        string savingPath = GameObject.Find("PageManager").GetComponent<CurrentPageManager>().savingPath;
        File.Delete(savingPath + SaveLoad.BOOKPREFIX + bookName + FILENAME + saveIndex + SaveLoad.SUFFIX);
    }
    public static bool CheckFolder(string folderName)
    {
        string savingPath = GameObject.Find("PageManager").GetComponent<CurrentPageManager>().savingPath;
        string path = savingPath + SaveLoad.BOOKPREFIX + folderName;
        if (Directory.Exists(path))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void CreateFolder(string folderName)
    {
        string path = SaveLoad.SAVEPREFIX + SaveLoad.BOOKPREFIX + folderName;
        Directory.CreateDirectory(path);
    }
    public static string GetSavingPath()
    {
        return SaveLoad.SAVEPREFIX;
    }
    public static string GetSavingPath(string bookName)
    {
        string savingPath = GameObject.Find("PageManager").GetComponent<CurrentPageManager>().savingPath;
        return (savingPath + SaveLoad.BOOKPREFIX + bookName);
    }
    public static string GetSavingPath(string bookName, int saveIndex)
    {
        return (SaveLoad.SAVEPREFIX + SaveLoad.BOOKPREFIX + bookName + FILENAME + saveIndex + SaveLoad.SUFFIX);
    }
    public static char GetSavingPrefix()
    {
        return SaveLoad.BOOKPREFIX;
    }
    public static string GetSavingFilename()
    {
        return FILENAME;
    }
    public static string GetSavingSuffix()
    {
        return SaveLoad.SUFFIX;
    }
}
