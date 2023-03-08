using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollerInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.GetComponent<ContentSizeFitter>().enabled = false;
        this.transform.position = Vector3.zero;
        this.gameObject.GetComponent<ContentSizeFitter>().enabled = true;
    }
}
