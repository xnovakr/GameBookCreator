using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProperty
{
    void DestroyMe(List<GameObject> list);
    void SetName(string name);
    void SetValue(int value);
    void SetValue(bool value);
    void SetID(int value);
    string GetName();
    string GetTypeName();
}
