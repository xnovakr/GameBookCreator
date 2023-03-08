using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPath : MonoBehaviour
{
    public int ID;
    public string connectingFile;
    public Conditions conditions = new Conditions();
    public Effects effects = new Effects();
    public string storyChoiseText;

    public void DestroyMe(List<GameObject> pathsList)
    {
        pathsList.Remove(gameObject);
        conditions.skillActions.Clear();
        conditions.artefactActions.Clear();
        effects.skillActions.Clear();
        effects.artefactActions.Clear();
        Destroy(gameObject);
        GameObject.Find("PageManager").GetComponent<CurrentPageManager>().UpdateObjectList(pathsList);
    }
    public void SetName(string name)
    {
        this.name = name;
    }
    public void SetID(int ID)
    {
        this.ID = ID;
    }
    public string GetTypeName()
    {
        return "Path";
    }
    public List<GameObject> GetConditionsSkillActions()
    {
        return conditions.skillActions;
    }
    public void SetConditionsSkillActions(List<GameObject> skillActions)
    {
        conditions.skillActions = skillActions;
    }
    public List<GameObject> GetConditionsArtefactActions()
    {
        return conditions.artefactActions;
    }
    public void SetConditionsArtefactActions(List<GameObject> artefactActions)
    {
        conditions.artefactActions = artefactActions;
    }
    public List<GameObject> GetEffectsSkillActions()
    {
        return effects.skillActions;
    }
    public void SetEffectsSkillActions(List<GameObject> skillActions)
    {
        effects.skillActions = skillActions;
    }
    public List<GameObject> GetEffectsArtefactActions()
    {
        return effects.artefactActions;
    }
    public void SetEffectsArtefactActions(List<GameObject> artefactActions)
    {
        effects.artefactActions = artefactActions;
    }
    public void ClearConditions()
    {
        foreach(GameObject go in conditions.skillActions)
        {
            Destroy(go);
        }
        conditions.skillActions.Clear();
        foreach (GameObject go in conditions.artefactActions)
        {
            Destroy(go);
        }
        conditions.artefactActions.Clear();
    }
    public void ClearEffects()
    {
        foreach (GameObject go in effects.skillActions)
        {
            Destroy(go);
        }
        effects.skillActions.Clear();
        foreach (GameObject go in effects.artefactActions)
        {
            Destroy(go);
        }
        effects.artefactActions.Clear();
    }
}
public class Conditions
{
    public  List<GameObject> skillActions = new List<GameObject>();
    public  List<GameObject> artefactActions = new List<GameObject>();
}
public class Effects
{
    public  List<GameObject> skillActions = new List<GameObject>();
    public  List<GameObject> artefactActions = new List<GameObject>();
}
public class MyPathPrefab
{
    public int ID;
    public string connectingFile;
    public string storyChoiseText;
}