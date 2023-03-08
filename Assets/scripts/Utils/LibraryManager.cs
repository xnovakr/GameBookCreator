using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using SimpleFileBrowser;

public class LibraryManager : MonoBehaviour
{
    private static string CACHEFILENAME = "chache.osas";
    private static string PLAYERLOG = "Player.log";
    private static string PLAYERLOGPREV = "Player-prev.log";

    private static string LANGUAGELIBRARY = "Library_Lang.xml";

    public GameObject Mask;
    public GameObject savingPathObject;
    public GameObject BookPrefab;
    public GameObject DestroyingBookWarning;
    public GameObject MovingFilesWarning;
    public GameObject LanguageDropdown;

    public CacheLibrary cacheObject = new CacheLibrary();

    private List<GameObject> books = new List<GameObject>();

    private string previousSavingPath;
    public string savingPath = "";
    string selectedLanguage = "English";

    private string[] languages = new string[3] { "English", "Slovak", "Czech" };

    // Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
    // Warning: FileBrowser can only show 1 dialog at a time

    private void Awake()
    {
        savingPath = SaveLoad.SAVEPREFIX + SaveLoad.BOOKPREFIX;
        previousSavingPath = savingPath;
        LoadChache();
        LoadBooks();
        savingPathObject.transform.Find("TextBackground").transform.Find("TextCurrentSavingPath").GetComponent<Text>().text = savingPath;
    }
    void Start()
    {
        List<Dropdown.OptionData> languagesDropdownData = new List<Dropdown.OptionData>();
        foreach(string lang in languages)
        {
            Dropdown.OptionData data = new Dropdown.OptionData();
            data.text = lang;
            languagesDropdownData.Add(data);
        }
        selectedLanguage = cacheObject.language;
        LanguageDropdown.GetComponent<Dropdown>().options = languagesDropdownData;
        LanguageDropdown.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { UpdateTexts(); });
        int indexOfLanguage = 0;
        foreach(Dropdown.OptionData data in LanguageDropdown.GetComponent<Dropdown>().options)
        {
            if (data.text == selectedLanguage) break;
            indexOfLanguage++;
        }
        LanguageDropdown.GetComponent<Dropdown>().value = indexOfLanguage;

        UpdateTexts();
        MovingFilesWarning.SetActive(false);
        DestroyingBookWarning.SetActive(false);
        GameObject.Find("WarningPanel").SetActive(false);
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Text Files", ".txt", ".pdf"));

        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        FileBrowser.SetDefaultFilter(".jpg");
        
        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
        FileBrowser.AddQuickLink("Default saving path", SaveLoad.SAVEPREFIX, null);

        // Show a save file dialog 
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Allow multiple selection: false
        // Initial path: "C:\", Title: "Save As", submit button text: "Save"
        // FileBrowser.ShowSaveDialog( null, null, false, false, "C:\\", "Save As", "Save" );

        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Allow multiple selection: false
        // Initial path: default (Documents), Title: "Select Folder", submit button text: "Select"
        // FileBrowser.ShowLoadDialog( ( paths ) => { Debug.Log( "Selected: " + paths[0] ); },
        //                            () => { Debug.Log( "Canceled" ); },
        //                            true, false, null, "Select Folder", "Select" );

    }
    public void UpdateTexts()
    {
        selectedLanguage = languages[LanguageDropdown.GetComponent<Dropdown>().value];
        LanguageHandler.LanguageLoader(selectedLanguage, LANGUAGELIBRARY);
        cacheObject.language = selectedLanguage;
        SaveChache();
        ReloadLibrary();
        
    }
    public void LoadBooks()
    {
        string[] files = Directory.GetDirectories(savingPath);
        foreach (string file in files)
        {
            int splitLenght = file.Split(/*SaveLoad.BOOKPREFIX*/'\\').Length;
            string fileName = file.Split(/*SaveLoad.BOOKPREFIX*/'\\')[splitLenght - 1];
            //if (fileName.Split('\\').Length > 1) fileName = fileName.Split('\\')[1];
            AddBook(fileName);
        }
    }
    public void AddBook(string name = null)
    {
        GameObject temp = Instantiate(BookPrefab, Vector3.zero, Quaternion.identity);// initialization of file prefab
        temp.transform.SetParent(Mask.transform.Find("Content").transform);// setting parent of prefab to content
        temp.transform.localScale = Vector3.one;// normalizing size of prefab

        Button editButton = temp.transform.Find("ButtonEdit").GetComponent<Button>();// getting button
        Button exportButton = temp.transform.Find("ButtonExport").GetComponent<Button>();
        Button deleteButton = temp.transform.Find("ButtonDelete").GetComponent<Button>();

        editButton.onClick.AddListener(delegate { EditBook(temp); });// setting up button
        exportButton.onClick.AddListener(delegate { BookExporter.ExportBook(temp); });
        deleteButton.onClick.AddListener(delegate { DeleteBook(temp); });

        InputField inputFieldName = temp.transform.Find("InputFieldName").GetComponent<InputField>();
        inputFieldName.onEndEdit.AddListener(delegate { RenameBook(temp, inputFieldName); /*ExportBook(exportButton, inputFieldName.text);*/ });

        int i = 0;
        foreach (GameObject book in books)
        {
            if ((name != null && name.Length > 0) && book.transform.name == name) i++;
            else if ((name != null && name.Length == 0) && book.transform.name == books.Count.ToString()) i++;
        }
        if (name != null && name.Length > 0)
        {
            if (i > 0) name += i.ToString();
            temp.transform.name = name;
            inputFieldName.text = name;
            //ExportBook(exportButton, name);
        }
        else
        {
            name = books.Count.ToString();
            if (i > 0) name += i.ToString();
            Directory.CreateDirectory(savingPath + SaveLoad.BOOKPREFIX + name);
            temp.transform.name = name;
            //ExportBook(exportButton, name);
        }

        //setting up text with current langugage
        string tempName = LanguageHandler.GetTextTranslation(selectedLanguage, "PlaceholderBookName", LANGUAGELIBRARY);
        if (tempName != null) temp.transform.Find("InputFieldName").Find("PlaceholderBookName").GetComponent<Text>().text = tempName;
        else Debug.Log("Failed to load InputFieldName translation for tex.");
        tempName = LanguageHandler.GetTextTranslation(selectedLanguage, "TextBookEdit", LANGUAGELIBRARY);
        if (tempName != null) temp.transform.Find("ButtonEdit").Find("TextBookEdit").GetComponent<Text>().text = tempName;
        else Debug.Log("Failed to load ButtonEdit translation for tex.");
        tempName = LanguageHandler.GetTextTranslation(selectedLanguage, "TextBookExport", LANGUAGELIBRARY);
        if (tempName != null) temp.transform.Find("ButtonExport").Find("TextBookExport").GetComponent<Text>().text = tempName;
        else Debug.Log("Failed to load ButtonExport translation for tex.");
        tempName = LanguageHandler.GetTextTranslation(selectedLanguage, "TextBookDelete", LANGUAGELIBRARY);
        if (tempName != null) temp.transform.Find("ButtonDelete").Find("TextBookDelete").GetComponent<Text>().text = tempName;
        else Debug.Log("Failed to load ButtonDelete translation for tex.");

        //ExportBook(exportButton, temp.transform.name);
        books.Add(temp);
        Mask.transform.Find("Content").transform.Find("BookAdd").transform.SetAsLastSibling();// moving adding button to end of groap
    }
    public void EditBook(GameObject book)
    {
        PlayerPrefs.SetString("book", book.transform.name); // saving book name to playerPrefs

        string[] filesInBook = Directory.GetFiles(savingPath + SaveLoad.BOOKPREFIX + book.name);
        if (filesInBook.Length > 0)
        {
            PlayerPrefs.SetInt("firstPage", 0); // saving books first page for loading
        }
        else
        {
            PlayerPrefs.SetInt("firstPage", -1); // saving books first page to null for creating new page
        }

        SceneManager.LoadScene("BookWritter");
    }
    public void ExportBook(Button exportButton, string name = null)
    {
        exportButton.onClick.RemoveAllListeners();
        Debug.Log("exporting");
        //if (name == null) exportButton.onClick.AddListener(delegate { BookExporter.ExportBook(savingPath + SaveLoad.BOOKPREFIX + name); });
        //else exportButton.onClick.AddListener(delegate { BookExporter.ExportBook(savingPath + SaveLoad.BOOKPREFIX + name); });
    }
    public void DeleteBook(GameObject temp)
    {
        if (Directory.GetFiles(savingPath + SaveLoad.BOOKPREFIX + temp.transform.name).Length <= 0)
        {
            books.Remove(temp);
            Directory.Delete(savingPath + SaveLoad.BOOKPREFIX + temp.transform.name);
            Destroy(temp.gameObject);
        }
        else
        {
            DestroyingBookWarning.transform.parent.gameObject.SetActive(true);
            DestroyingBookWarning.SetActive(true);
            DestroyingBookWarning.transform.Find("ButtonYes").GetComponent<Button>().onClick.AddListener(delegate { DeleteBookWithFiles(temp); });
        }
    }
    public void DeleteBookWithFiles(GameObject book)
    {
        string[] filesInBook = Directory.GetFiles(savingPath + SaveLoad.BOOKPREFIX + book.transform.name);
        foreach (string file in filesInBook)
        {
            File.Delete(file);
        }
        books.Remove(book);
        Directory.Delete(savingPath + SaveLoad.BOOKPREFIX + book.transform.name);
        Destroy(book);
        book.SetActive(false);
        DestroyingBookWarning.SetActive(false);
        DestroyingBookWarning.transform.parent.gameObject.SetActive(false);
        DestroyingBookWarning.transform.Find("ButtonYes").GetComponent<Button>().onClick.RemoveAllListeners();
    }
    public void RenameBook(GameObject book, InputField inputFieldName)
    {
        if (book.transform.name == inputFieldName.text) return;
        string newName = "";
        foreach (string nameChunk in inputFieldName.text.Split('\n'))// replacing enter for spacebar
        {
            newName += nameChunk + " ";
        }
        if (Directory.Exists(savingPath + newName)) return;
        Directory.Move(savingPath + SaveLoad.BOOKPREFIX + book.transform.name, savingPath + SaveLoad.BOOKPREFIX + newName);
        book.transform.name = newName;
    }
    public void CloseWarning(GameObject warning)
    {
        warning.SetActive(false);
        warning.transform.parent.gameObject.SetActive(false);
    }
    public void SetSavingPath(string savingPath)
    {
        this.savingPath = savingPath;
    }
    public string GetSavingPath()
    {
        return savingPath;
    }
    public void CloseApplication()
    {
        Application.Quit();
    }
    public void ReloadLibrary()
    {
        foreach (GameObject book in books)
        {
            Destroy(book);
        }
        books.Clear();
        LoadBooks();
        ReloadSavingPathText();
    }
    public void OpenSavingFolder()
    {
        System.Diagnostics.Process.Start(@savingPath);
    }
    public void SetDefaultPath()
    {
        string newPath = SaveLoad.SAVEPREFIX;
        if (cacheObject == null) cacheObject = new CacheLibrary();
        cacheObject.savingPath = newPath;
        SaveChache();
        previousSavingPath = savingPath;
        MovingFilesWarning.transform.parent.gameObject.SetActive(true);
        MovingFilesWarning.SetActive(true);
        MovingFilesWarning.transform.Find("ButtonYes").GetComponent<Button>().onClick.AddListener(delegate { MoveAllFiles(previousSavingPath); });
        savingPath = newPath;
        savingPathObject.transform.Find("TextBackground").transform.Find("TextCurrentSavingPath").GetComponent<Text>().text = savingPath;
        ReloadLibrary();
    }
    public void SelectCustomFolder()
    {
        // Coroutine example
        StartCoroutine(ShowLoadDialogCoroutine());
    }
    public void MoveAllFiles(string oldPath)
    {
        if (oldPath == savingPath)
        {
            CloseWarning(MovingFilesWarning);
            return;
        }
        string[] directories = Directory.GetDirectories(oldPath);
        string[] files = Directory.GetFiles(oldPath);
        foreach(string directory in directories)
        {
            Directory.Move(directory, savingPath + SaveLoad.BOOKPREFIX + directory.Split('\\')[directory.Split('\\').Length - 1]);
        }
        foreach (string file in files)
        {
            if (file.Split('\\')[file.Split('\\').Length - 1] == CACHEFILENAME) continue;
            if (file.Split('\\')[file.Split('\\').Length - 1] == PLAYERLOG) continue;
            if (file.Split('\\')[file.Split('\\').Length - 1] == PLAYERLOGPREV) continue;
            Debug.Log(file);
            File.Move(file, savingPath + SaveLoad.BOOKPREFIX + file.Split('\\')[file.Split('\\').Length - 1]);
        }
        CloseWarning(MovingFilesWarning);
        ReloadLibrary();
    }
    public void UpdateCacheData()
    {
        cacheObject.savingPath = savingPath;
    }
    public void SaveChache()
    {
        string cachePath = SaveLoad.SAVEPREFIX + SaveLoad.BOOKPREFIX + CACHEFILENAME;
        string cacheData = JsonUtility.ToJson(cacheObject);
        File.WriteAllText(cachePath, cacheData);
    }
    public void LoadChache()
    {
        string cachePath = SaveLoad.SAVEPREFIX + SaveLoad.BOOKPREFIX + CACHEFILENAME;
        if (!File.Exists(cachePath))
        {
            Debug.Log("Cache file don't exist.");
            return;
        }
        string loadedCache = File.ReadAllText(cachePath);
        cacheObject = JsonUtility.FromJson<CacheLibrary>(loadedCache);
        if (cacheObject != null && cacheObject.savingPath != null &&
            cacheObject.savingPath.Length > 3 && cacheObject.savingPath != savingPath) savingPath = cacheObject.savingPath;
    }
    public void ReloadSavingPathText()
    {
        Canvas.ForceUpdateCanvases();
        savingPathObject.transform.Find("TextBackground").transform.Find("TextCurrentSavingPath").GetComponent<ContentSizeFitter>().enabled = false;
        savingPathObject.transform.Find("TextBackground").transform.Find("TextCurrentSavingPath").GetComponent<ContentSizeFitter>().enabled = true;

        Canvas.ForceUpdateCanvases();
        savingPathObject.transform.Find("TextBackground").GetComponent<ContentSizeFitter>().enabled = false;
        savingPathObject.transform.Find("TextBackground").GetComponent<HorizontalLayoutGroup>().enabled = false;
        savingPathObject.transform.Find("TextBackground").GetComponent<ContentSizeFitter>().enabled = true;
        savingPathObject.transform.Find("TextBackground").GetComponent<HorizontalLayoutGroup>().enabled = true;
    }
    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Allow multiple selection: true
        // Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(true, false, null, "Load File", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        //Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++) { }
                //Debug.Log(FileBrowser.Result[i]);

            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            //byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);



            string newPath = "";
            //FileBrowser.ShowLoadDialog(onSuccess,onCancel);

            if (FileBrowser.Success) newPath = FileBrowser.Result[0];

            //ShowLoadDialogCoroutine();
            if (Directory.Exists(newPath))
            {
                //Debug.Log("Legit folder " + newPath);

                if (newPath + SaveLoad.BOOKPREFIX != savingPath)
                {
                    if (cacheObject == null) cacheObject = new CacheLibrary();
                    cacheObject.savingPath = newPath;
                    SaveChache();
                    previousSavingPath = savingPath;
                    MovingFilesWarning.transform.parent.gameObject.SetActive(true);
                    MovingFilesWarning.SetActive(true);
                    MovingFilesWarning.transform.Find("ButtonYes").GetComponent<Button>().onClick.AddListener(delegate { MoveAllFiles(previousSavingPath); });
                    savingPath = newPath;
                    savingPathObject.transform.Find("TextBackground").transform.Find("TextCurrentSavingPath").GetComponent<Text>().text = savingPath;
                }
                ReloadLibrary();
            }
            else
            {
                Debug.Log(newPath);
                Debug.Log("Selected folred is non existant or from other dimension");
            }
        }
    }
}
