using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Skill : MonoBehaviour, IProperty
{
    public new string name;
    public int matematicalOperation = 0; // 0 = addition  1 = subtraction
    public int value;
    public int ID;
    public string groapName;

    public Skill(string name, int value)
    {
        this.name = name;
        this.value = value;
    }
    public SkillProp CreateProp(Skill skill)
    {
        return new SkillProp(skill.name, skill.value, skill.ID);
    }
    public void DestroyMe(List<GameObject> skillsList)
    {
        skillsList.Remove(gameObject);
        Destroy(gameObject);
        //GameObject.Find("PageManager").GetComponent<CurrentPageManager>().UpdateObjectList(skillsList);
    }
    public void SetName(string name)
    {
        this.name = name;
    }
    public void SetValue(int value)
    {
        this.value = value;
    }
    public void SetValue(bool value)
    {
        Debug.Log("Skill don't have bool value.");
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
        return "Skill";
    }
}
public class SkillProp
{
    public string name;
    public int value;
    public int ID;
    public string groapName;
    public int matematicalOperation; // 0 = addition  1 = subtraction
    public SkillProp(string name, int value, int ID, string groapName = null, int matematicalOperation = 0)
    {
        this.name = name;
        this.value = value;
        this.ID = ID;
        this.groapName = groapName;
        this.matematicalOperation = matematicalOperation;
    }
}