using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class SaveLoadPageProperties
{
    private const char PATHSEPARATOR = '#';
    private const char PROPERTYESSEPARATOR = '|';
    private const char GROAPSEPARATOR = '@';
    public const string SKILLS = "skills";
    public const string ARTEFACTS = "artefacs";
    public const string PATHS = "paths-";
    private const string CONDITIONS = "Conditions";
    private const string EFFECTS = "Effects";
    public static void SavePaths(string book, string pageIndex, List<GameObject> paths)
    {
        string path = SaveLoadPage.GetSavingPath(book) + '\\' + PATHS + pageIndex + SaveLoadPage.GetSavingSuffix();
        string json = "";
        // creating json string of all artefacts
        foreach (GameObject obj in paths)
        {
            //Debug.Log("saving");
            if (json.Length > 1) json += PATHSEPARATOR;
            json += JsonUtility.ToJson(obj.GetComponent<MyPath>());
            json += GROAPSEPARATOR + SaveCondtions(obj) + GROAPSEPARATOR + SaveEffects(obj) + GROAPSEPARATOR + obj.transform.Find("Death toggle").Find("Toggle").GetComponent<Toggle>().isOn;
        }
        File.WriteAllText(path, json);
    }
    public static string SaveCondtions(GameObject obj)
    {
        return SaveLoadBookProperties.SaveSkills(obj.GetComponent<MyPath>().GetConditionsSkillActions()) + PROPERTYESSEPARATOR
             + SaveLoadBookProperties.SaveArtefacts(obj.GetComponent<MyPath>().GetConditionsArtefactActions());
    }
    public static string SaveEffects(GameObject obj)
    {
        return SaveLoadBookProperties.SaveSkills(obj.GetComponent<MyPath>().GetEffectsSkillActions()) + PROPERTYESSEPARATOR
             + SaveLoadBookProperties.SaveArtefacts(obj.GetComponent<MyPath>().GetEffectsArtefactActions());
    }
    public static void LoadPaths(string bookName, string pageIndex)
    {
        string filePath = SaveLoadPage.GetSavingPath(bookName) + '\\' + PATHS + pageIndex + SaveLoadPage.GetSavingSuffix();
        if (!SaveLoad.CheckFile(filePath)) return;
        string loadedText = File.ReadAllText(filePath);
        if (loadedText.Length < 2) return;
        string[] paths = loadedText.Split(PATHSEPARATOR);
        foreach (string path in paths)
        {
            MyPathPrefab currentPath = JsonUtility.FromJson<MyPathPrefab>(path.Split(GROAPSEPARATOR)[0]);
            string conditions = path.Split(GROAPSEPARATOR)[1];
            string effects = path.Split(GROAPSEPARATOR)[2];
            bool deathEnd = bool.Parse(path.Split(GROAPSEPARATOR)[3]);

            GameObject.Find("PageManager").GetComponent<CurrentPageManager>().CreatePath(currentPath.ID, currentPath, deathEnd);
            GameObject currentPathObject = GameObject.Find("PageManager").GetComponent<CurrentPageManager>().paths[currentPath.ID];

            LoadCondtions(currentPathObject, conditions);
            LoadEffects(currentPathObject, effects);
        }
    }
    public static void LoadCondtions(GameObject path, string conditions)
    {
        if (conditions.Length < 1) return;
        string jsonSkills = conditions.Split(PROPERTYESSEPARATOR)[0];
        string jsonArtefacts = conditions.Split(PROPERTYESSEPARATOR)[1];
        LoadSkillActions(path, jsonSkills);
        LoadArtefactActions(path, jsonArtefacts);
    }
    public static void LoadEffects(GameObject path, string conditions)
    {
        if (conditions.Length < 1) return;
        string jsonSkills = conditions.Split(PROPERTYESSEPARATOR)[0];
        string jsonArtefacts = conditions.Split(PROPERTYESSEPARATOR)[1];
        LoadSkillActions(path, jsonSkills);
        LoadArtefactActions(path, jsonArtefacts);
    }
    public static void LoadSkillActions(GameObject path, string skills)
    {
        string[] jsonSkills = skills.Split(SaveLoadBookProperties.SEPARATOR);
        // goning thrue all json save files
        foreach (string json in jsonSkills)
        {
            if (json.Length < 3) continue;// if file is too short to be json file continue in cyclus
            SkillProp skill = JsonUtility.FromJson<SkillProp>(json);
            if (skill.groapName == CONDITIONS)
            {
                GameObject.Find("PageManager").GetComponent<CurrentPageManager>().CreateSkillAction(path.GetComponent<MyPath>().GetConditionsSkillActions(), path, skill.groapName, skill.ID, skill.name, skill.value, skill.matematicalOperation);
            }
            else if (skill.groapName == EFFECTS)
            {
                GameObject.Find("PageManager").GetComponent<CurrentPageManager>().CreateSkillAction(path.GetComponent<MyPath>().GetEffectsSkillActions(), path, skill.groapName, skill.ID, skill.name, skill.value, skill.matematicalOperation);
            }
        }
    }
    public static void LoadArtefactActions(GameObject path, string artefacts)
    {
        string[] jsonArtefacts = artefacts.Split(SaveLoadBookProperties.SEPARATOR);
        // goning thrue all json save files
        foreach (string json in jsonArtefacts)
        {
            if (json.Length < 3) continue;// if file is too short to be json file continue in cyclus
            ArtefactProp artefact = JsonUtility.FromJson<ArtefactProp>(json);
            if (artefact.category == CONDITIONS)
            {
                GameObject.Find("PageManager").GetComponent<CurrentPageManager>().CreateArtefactAction(path.GetComponent<MyPath>().GetConditionsArtefactActions(), path, artefact.category, artefact.ID, artefact.name, artefact.isActive);
            }
            else if (artefact.category == EFFECTS)
            {
                GameObject.Find("PageManager").GetComponent<CurrentPageManager>().CreateArtefactAction(path.GetComponent<MyPath>().GetEffectsArtefactActions(), path, artefact.category, artefact.ID, artefact.name, artefact.isActive);
            }
        }
    }
}
