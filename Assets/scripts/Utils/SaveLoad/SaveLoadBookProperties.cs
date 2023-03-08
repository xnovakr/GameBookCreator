using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class SaveLoadBookProperties
{
    public const char SEPARATOR = '$';
    private const string SKILLS = "skills";
    private const string ARTEFACTS = "artefacs";
    private static CurrentPageManager pageManager = GameObject.Find("PageManager").GetComponent<CurrentPageManager>();
    
    public static void SaveSkills(string book, List<GameObject> skills)
    {
        string savingPath = GameObject.Find("PageManager").GetComponent<CurrentPageManager>().savingPath;
        string path = savingPath + SaveLoad.BOOKPREFIX + book + '\\' + SKILLS + SaveLoadPage.GetSavingSuffix();
        string json = "";
        // creating json string of all skills
        foreach (GameObject obj in skills)
        {
            json += JsonUtility.ToJson(obj.GetComponent<Skill>()) + SEPARATOR;
        }
        File.WriteAllText(path, json);
    }
    public static string SaveSkills(List<GameObject> skills)
    {
        string json = "";
        // creating json string of all skills
        foreach (GameObject obj in skills)
        {
            if (json.Length > 1) json += SEPARATOR;
            json += JsonUtility.ToJson(obj.GetComponent<Skill>());
        }
        return json;
    }
    public static void LoadSkills(string book)
    {
        string path = SaveLoadPage.GetSavingPath(book) + '\\' + SKILLS + SaveLoadPage.GetSavingSuffix();
        if (!SaveLoad.CheckFile(path)) return;
        string loadedText = File.ReadAllText(path);
        string[] jsonSkills = loadedText.Split(SEPARATOR);
        // goning thrue all json save files
        foreach (string json in jsonSkills)
        {
            if (json.Length < 3) continue;// if file is too short to be json file continue in cyclus
            SkillProp skill = JsonUtility.FromJson<SkillProp>(json);
            GameObject.Find("PageManager").GetComponent<CurrentPageManager>().CreateSkill(skill.ID, skill.name, skill.value);
        }
    }
    public static void SaveArtefacts(string book, List<GameObject> artefacts)
    {
        string path = SaveLoadPage.GetSavingPath(book) + '\\' + ARTEFACTS + SaveLoadPage.GetSavingSuffix();
        string json = "";
        // creating json string of all artefacts
        foreach (GameObject obj in artefacts)
        {
            if (json.Length > 1) json += SEPARATOR;
            json += JsonUtility.ToJson(obj.GetComponent<Artefact>());
        }

        //Debug.Log(json);
        File.WriteAllText(path, json);
    }
    public static string SaveArtefacts(List<GameObject> artefacts)
    {
        string json = "";
        // creating json string of all artefacts
        foreach (GameObject obj in artefacts)
        {
            if (json.Length > 1) json += SEPARATOR;
            json += JsonUtility.ToJson(obj.GetComponent<Artefact>());
        }
        return json;
    }
    public static void LoadArtefacts(string book)
    {
        string path = SaveLoadPage.GetSavingPath(book) + '\\' + ARTEFACTS + SaveLoadPage.GetSavingSuffix();
        if (!SaveLoad.CheckFile(path)) return;
        string loadedText = File.ReadAllText(path);
        string[] jsonArtefacts = loadedText.Split(SEPARATOR);
        foreach (string json in jsonArtefacts)
        {
            if (json.Length < 3) continue;
            ArtefactProp artefact = JsonUtility.FromJson<ArtefactProp>(json);
            GameObject.Find("PageManager").GetComponent<CurrentPageManager>().CreateArtefact(artefact.ID, artefact.name, artefact.isActive);
        }
    }
}