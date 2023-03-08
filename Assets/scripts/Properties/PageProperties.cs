using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageProperties : MonoBehaviour
{
    public void LoadMe()
    {
        //Debug.Log(this.transform.name);
        GameObject.Find("PageManager").GetComponent<CurrentPageManager>().LoadNewPage(transform.name);

    }
}
