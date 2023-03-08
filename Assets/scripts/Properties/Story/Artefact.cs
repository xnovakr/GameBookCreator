using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Artefact : MonoBehaviour, IProperty
{
    public new string name;
    public bool isActive;
    public int ID;
    public string category;
    public Artefact(string name, bool isActive)
    {
        this.name = name;
        this.isActive = isActive;
    }
    public ArtefactProp CreateProp(Artefact artefact)
    {
        return new ArtefactProp(artefact.name, artefact.isActive, artefact.ID);
    }
    public void DestroyMe(List<GameObject> artefactList)
    {
        artefactList.Remove(gameObject);
        Destroy(gameObject);
        //GameObject.Find("PageManager").GetComponent<CurrentPageManager>().UpdateObjectList(artefactList);
    }
    public void SetName(string name)
    {
        this.name = name;
    }
    public void SetValue(int value)
    {
        if (value == 0) isActive = false;
        else if (value == 1) isActive = true;
        else Debug.Log("Artefacts don't have int value. " + value);
    }
    public void SetValue(string value)
    {
        if (value == "False") isActive = false;
        else if (value == "True") isActive = true;
        else Debug.Log("Artefacts don't have int value. " + value);
    }
    public void SetValue(bool value)
    {
        this.isActive = value;
    }
    public void SetID(int value)
    {
        this.ID = value;
    }

    public string GetName()
    {
        return name;
    }
    public string GetTypeName()
    {
        return "Artefact";
    }
}

public class ArtefactProp
{
    public string name;
    public bool isActive;
    public int ID;
    public string category;

    public ArtefactProp(string name, bool isActive, int ID, string category = null)
    {
        this.name = name;
        this.isActive = isActive;
        this.ID = ID;
        this.category = category;
    }
}
