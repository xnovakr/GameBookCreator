using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyBook : MonoBehaviour
{
    public void DestroyMe(List<GameObject> bookList)
    {
        if (Directory.GetFiles(SaveLoadPage.GetSavingPath(this.gameObject.transform.name)).Length <= 0)
        {
            bookList.Remove(this.gameObject);
            Directory.Delete(SaveLoadPage.GetSavingPath(this.gameObject.transform.name));
            Destroy(this.gameObject);
        }
        else
        {
            GameObject.Find("Library Manager").GetComponent<LibraryManager>().DestroyingBookWarning.transform.parent.gameObject.SetActive(true);
            GameObject.Find("Library Manager").GetComponent<LibraryManager>().DestroyingBookWarning.SetActive(true);
            Debug.Log(this.gameObject.transform.name);
            GameObject.Find("Library Manager").GetComponent<LibraryManager>().DestroyingBookWarning.transform.Find("ButtonYes").GetComponent<Button>().onClick.AddListener(delegate { DeleteBookWithFiles(bookList); });
        }
        //GameObject.Find("PageManager").GetComponent<CurrentPageManager>().UpdateObjectList(artefactList);
    }

    public void DeleteBookWithFiles(List<GameObject> books)
    {
        if (!this.gameObject)
        {
            Debug.Log("Game Object non existant");
            return;
        }
        else
        {
            Debug.Log("deleting with files");
        }
        string[] filesInBook = Directory.GetFiles(SaveLoadPage.GetSavingPath(this.transform.name));
        foreach (string file in filesInBook)
        {
            File.Delete(file);
        }
        books.Remove(this.gameObject);
        Directory.Delete(SaveLoadPage.GetSavingPath(this.gameObject.transform.name));
        Destroy(this.gameObject);
        this.gameObject.SetActive(false);
        GameObject.Find("Library Manager").GetComponent<LibraryManager>().DestroyingBookWarning.SetActive(false);
        GameObject.Find("Library Manager").GetComponent<LibraryManager>().DestroyingBookWarning.transform.parent.gameObject.SetActive(false);
        GameObject.Find("Library Manager").GetComponent<LibraryManager>().DestroyingBookWarning.transform.Find("ButtonYes").GetComponent<Button>().onClick.RemoveAllListeners();
    }
}
