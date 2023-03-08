using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CurrentPageManager : MonoBehaviour
{
    public static char TITLESEPARATOR = 'ˇ';
    public static string CACHEFILENAME = "chache.osas";
    public static string CHECKPOINTFILENAME = "checkpoints.osas";
    public static string ENDINGFILENAME = "endings.osas";
    public static string LANGUAGELIBRARY = "BookWriter_Lang.xml";

    public GameObject storyFilesObject;
    public GameObject skillsObject;
    public GameObject artefactsObject;
    public GameObject pathsObject;
    public GameObject bookPreviewObject;
    public GameObject bookInfoObject;
    public GameObject bookManagerObject;
    public GameObject warningObject;
    public GameObject checkpointObject;
    public GameObject endingObject;

    public InputField storyTextObject;
    public InputField titleTextObject;

    public GameObject storyFilePrefab;
    public GameObject skillPrefab;
    public GameObject skillActionPrefab;
    public GameObject artefactPrefab;
    public GameObject artefactAcrionPrefab;
    public GameObject pathPrefab;
    public GameObject storyChoisePrefab;

    public CacheLibrary cacheObject;

    public List<GameObject> artefacts = new List<GameObject>();
    public List<GameObject> skills = new List<GameObject>();
    public List<GameObject> paths = new List<GameObject>();
    public List<string> checkpoints = new List<string>();
    public List<string> endings = new List<string>();

    public string bookName;
    public string pageName;
    public string currentPageIndex;
    public string currentpageName;
    public string savingPath;
    public string language;

    private void Start()
    {
        //DontDestroyOnLoad(this);\
        LoadCache();
        language = cacheObject.language;
        LanguageHandler.LanguageLoader(language, LANGUAGELIBRARY);
        language = cacheObject.language;
        bookName = PlayerPrefs.GetString("book"); // loading book name from playerPrefs
        LoadCheckpoints();
        LoadEndings();
        LoadCurrentBook();
        titleTextObject.onEndEdit.AddListener(delegate { pageName = titleTextObject.text; UpdateBookInfo(); });
        UpdateSkills();
        UpdateArtefacts();
        UpdatePaths();
    }
    private void Update()
    {
        UpdateCanvasSorting(pathsObject);
    }
    public void LoadCurrentBook()
    {
        if (bookName[bookName.Length-1] == ' ')
        {
            Debug.Log("repairing book name bcs of spacebar");
            bookName = bookName.Remove(bookName.Length - 1, 1);
            //string fixedBookName = "";
            //for (int i = 0; i < bookName.Length-2; i++)
            //{
            //    fixedBookName += bookName[i];
            //}
            //bookName = fixedBookName;
        }

        string[] files = GetStoryFiles();
        List<string> storyFiles = new List<string>();
        foreach (string file in files)
        {
            string fileName = file.Split('\\')[file.Split('\\').Length - 1]; // extracting filename from path
            if (fileName.StartsWith("page")) storyFiles.Add(fileName);
        }
        if(storyFiles.Count > 0)
        {
            string pageName = storyFiles[0].Split('-')[1].Split('.')[0];
            foreach (string storyFile in storyFiles)
            {
                if (storyFile.Contains("prologue")) pageName = "prologue";
            }
            if (pageName != "prologue") CreateNewPage("prologue");
            LoadNewPage("prologue", true);
        }
        //else if (PlayerPrefs.GetInt("firstPage") == 0)// first page is already created so just loading it
        //{
        //    currentPageIndex = 0.ToString();
        //    LoadNewPage(currentPageIndex, true);
        //}
        else // first page is not cerated yet so creating it
        {
            CreateNewPage("prologue");
            LoadNewPage("prologue", true);
        }
        UpdateBookInfo(); //Update book name, page name and page index in UI
    }
    public void UpdateStoryFiles()
    {
        string[] files = GetStoryFiles();
        List<string> storyFiles = new List<string>();
        foreach (string file in files)
        {
            string fileName = file.Split('\\')[file.Split('\\').Length - 1]; // extracting filename from path
            if (fileName.StartsWith("page")) storyFiles.Add(file);
        }
        storyFilesObject.transform.Find("ButtonCreateFile").GetComponent<Button>().onClick.RemoveAllListeners();
        storyFilesObject.transform.Find("ButtonCreateFile").GetComponent<Button>().onClick.AddListener(delegate { CreateNewPage(storyFiles.Count); /*Debug.Log(storyFiles.Count);*/ });
        RemoveStoryFiles();
        foreach (string file in storyFiles)
        {
            //Debug.Log(file);
            string fileName = file.Split('\\')[file.Split('\\').Length - 1]; // extracting filename from path
            if (!fileName.StartsWith("page")) continue;
            GameObject temp = Instantiate(storyFilePrefab, Vector3.zero, Quaternion.identity); // initialization of file prefab
            temp.transform.SetParent(storyFilesObject.transform.Find("ContentMask").transform.Find("Content").transform); // setting parent of prefab to content
            fileName = fileName.Split('.')[0];
            fileName = fileName.Split('-')[1];
            temp.transform.name = fileName; // setting name of prefab to index of save
            Button buttonEdit = temp.transform.Find("ButtonEdit").GetComponent<Button>(); //getting button
            Button buttonDelete = temp.transform.Find("ButtonDelete").GetComponent<Button>(); //getting button

            InputField inputFieldName = temp.transform.Find("InputFieldFileName").GetComponent<InputField>();
            inputFieldName.text = fileName;
            inputFieldName.onEndEdit.AddListener(delegate { RenamePage(temp, inputFieldName); });

            if (fileName == currentPageIndex.ToString())
            {
                buttonEdit.gameObject.SetActive(false);
                buttonDelete.gameObject.SetActive(false);
            }//if this file is open disable button
            else
            {
                buttonEdit.onClick.AddListener(temp.GetComponent<PageProperties>().LoadMe); // setting up loader of page connected to button
                buttonDelete.onClick.AddListener(delegate { DeletePage(fileName); });
            }
            if (fileName == "prologue") 
            {
                inputFieldName.interactable = false;
                buttonDelete.gameObject.SetActive(false);
            } 

            temp.transform.localScale = Vector3.one;
            //changing this is bugged and not working
            //storyFilesObject.transform.Find("ContentMask").GetComponent<ScrollRect>().verticalNormalizedPosition = 1; //set scrolling of files to top
            //skillsObject.transform.Find("ContentMask").GetComponent<ScrollRect>().verticalNormalizedPosition = 1; //set scrolling of skills to top
            //artefactsObject.transform.Find("ContentMask").GetComponent<ScrollRect>().verticalNormalizedPosition = 1; //set scrolling of artefacts to top
        }
        UpdateBookInfo();
    }//load names of all files this book is containing ad apply them in UI
    private void RemoveStoryFiles()
    {
        Transform filesParent =storyFilesObject.transform.Find("ContentMask").transform.Find("Content").transform;
        foreach ( Transform child in filesParent)
        {
            Destroy(child.gameObject);
        }
    }
    public void UpdateBookInfo()
    {
        bookInfoObject.GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "TagBookName", LANGUAGELIBRARY) + bookName + "   " + LanguageHandler.GetTextTranslation(language, "TagPageName", LANGUAGELIBRARY) + " " + currentPageIndex;
    }//Update book name, page name and page index in UI
    public string[] GetStoryFiles()
    {
        return Directory.GetFiles(savingPath + SaveLoad.BOOKPREFIX + bookName);
    }
    public void CreateNewPage(int index)
    {
        foreach (string file in GetStoryFiles())
        {
            string fileName = file.Split('\\')[file.Split('\\').Length - 1];
            if (!fileName.StartsWith("page")) continue;
            fileName = fileName.Split('-')[1].Split('.')[0];
            if (index.ToString() == fileName) index++;
        }
        File.Create(savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + SaveLoadPage.FILENAME + index + SaveLoad.SUFFIX).Close();
        UpdateStoryFiles();
    }
    public void CreateNewPage(string index)
    {
        File.Create(savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoadPage.FILENAME + index + SaveLoad.SUFFIX).Close();
        UpdateStoryFiles();
    }
    public void LoadNewPage(string index, bool firstPage = false)
    {
        if (!firstPage) SaveCurrentPage(); //if its not firts page them save it
        currentPageIndex = index;
        currentpageName = index;
        UpdateStoryFiles();
        string loadedText = SaveLoadPage.LoadPageText(bookName, index);
        if (loadedText != null)
        {
            string titleText = "";
            string storyText = loadedText;

            if (loadedText.Split(TITLESEPARATOR).Length - 1 > 1)
            {
                currentpageName = loadedText.Split(TITLESEPARATOR)[0];
                titleText = loadedText.Split(TITLESEPARATOR)[1];
                storyText = loadedText.Split(TITLESEPARATOR)[2];
            }// IF THERE IS TITLE AND STORY SEPERATE THEM OTHERVISE SAVE STORY

            //Debug.Log(loadedText);
            titleTextObject.text = titleText;
            pageName = titleText;
            storyTextObject.text = storyText;
            currentpageName = index;
        }
        Canvas.ForceUpdateCanvases();

        storyTextObject.GetComponent<ContentSizeFitter>().enabled = false;
        storyTextObject.GetComponent<ContentSizeFitter>().enabled = true;
        storyTextObject.GetComponent<VerticalLayoutGroup>().enabled = false;
        storyTextObject.GetComponent<VerticalLayoutGroup>().enabled = true;

        if (false) Debug.Log("Story save not found.");// implement file corruption
        if (false) Debug.Log("Paths save not found.");
        if (false) Debug.Log("Others save not found.");

        UpdatePaths();
        SetupCheckpoint();
        SetupEndig();
    }
    public void SaveCurrentPage()
    {
        //Debug.Log(currentpageName);
        string textToSave = currentpageName;
        textToSave += TITLESEPARATOR + titleTextObject.transform.Find("TitleTextField").GetComponent<Text>().text;
        textToSave += TITLESEPARATOR + storyTextObject.transform.Find("StoryTextField").GetComponent<Text>().text;

        SaveLoadPage.SavePageText(bookName, currentPageIndex, textToSave);

        SaveLoadBookProperties.SaveSkills(bookName, skills);
        SaveLoadBookProperties.SaveArtefacts(bookName, artefacts);

        SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths);
    }
    public void UpdateSkills()
    {
        DeleteSkills();
        skills.Clear();
        SaveLoadBookProperties.LoadSkills(bookName);
    }
    public void DeleteSkills()
    {
        foreach (GameObject skill in skills.ToArray())
        {
            skill.GetComponent<Skill>().DestroyMe(skills);
        }
    }
    public void AddSkill()
    {
        int skillID = skills.Count ;
        CreateSkill(skillID);

        //float contentHight = artefactsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<RectTransform>().rect.height;
        float heightOffset = 19 * skillID + 20;// cca artefact size (little smaller) * number of artefacts + buttonAddHeight
        skillsObject.transform.Find("ContentMask").transform.Find("Content").transform.localPosition = new Vector3(0, -heightOffset, 0); // setting up scrolling window at bottom
        RefreshPaths();
    }
    public void CreateSkill(int skillID, string skillName = null, int skillValue = 0)
    {
        if (!skillsObject) skillsObject = GameObject.Find("CharacterSkills");
        GameObject temp = Instantiate(skillPrefab, Vector3.zero, Quaternion.identity); // initialization of file prefab
        temp.transform.SetParent(skillsObject.transform.Find("ContentMask").transform.Find("Content").transform); // setting parent of prefab to content
        temp.transform.name = "Skill" + skillID;
        Button buttonDelete = temp.transform.Find("ButtonRemove").GetComponent<Button>(); //getting button
        buttonDelete.onClick.AddListener(delegate { temp.GetComponent<Skill>().DestroyMe(skills); SaveLoadBookProperties.SaveSkills(bookName, skills); }); // setting up destroyer of item connected to button
        temp.transform.localScale = Vector3.one;//normalizing size of prefab
        skillsObject.transform.Find("ContentMask").transform.Find("Content").transform.Find("ButtonAdd").transform.SetAsLastSibling();// moving adding button to end of groap

        InputField inputFieldName = temp.transform.Find("InputFieldSkillName").GetComponent<InputField>();
        InputField inputFieldValue = temp.transform.Find("InputFieldSkillValue").GetComponent<InputField>();
        inputFieldName.onEndEdit.AddListener(delegate { temp.GetComponent<Skill>().SetName(inputFieldName.text); temp.transform.name = inputFieldName.text; RefreshPaths(); SaveLoadBookProperties.SaveSkills(bookName, skills); }); // saving name of skill on change
        inputFieldValue.onEndEdit.AddListener(delegate { temp.GetComponent<Skill>().SetValue(int.Parse(inputFieldValue.text)); SaveLoadBookProperties.SaveSkills(bookName, skills); }); // saving value of skill on change
        temp.transform.Find("InputFieldSkillName").Find("PlaceholderSkillName").GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "PlaceholderSkillName", LANGUAGELIBRARY);

        if (skillName != null)
        {
            temp.GetComponent<Skill>().name = skillName;
            temp.GetComponent<Skill>().value = skillValue;
            inputFieldName.text = skillName;
            inputFieldValue.text = skillValue.ToString();
            temp.transform.name = skillName;
        }

        temp.GetComponent<Skill>().SetID(skillID);
        skills.Add(temp);//adding to array of all skills
        SaveLoadBookProperties.SaveSkills(bookName, skills);
    }
    public void UpdateArtefacts()
    {
        DeleteArtefacts();
        artefacts.Clear();
        SaveLoadBookProperties.LoadArtefacts(bookName);
    }
    public void DeleteArtefacts()
    {
        foreach (GameObject artefact in artefacts.ToArray())
        {
            artefact.GetComponent<Artefact>().DestroyMe(artefacts);
        }
    }
    public void AddArtefact()
    {
        int artefactID = artefacts.Count;
        CreateArtefact(artefactID);

        //float contentHight = artefactsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<RectTransform>().rect.height;
        float heightOffset = 20 * artefactID + 20;// cca artefact size (little smaller) * number of artefacts + buttonAddHeight
        artefactsObject.transform.Find("ContentMask").transform.Find("Content").transform.localPosition = new Vector3(0, -heightOffset, 0); // setting up scrolling window at bottom
        RefreshPaths();
    }
    public void CreateArtefact(int artefactID, string artefactName = null, bool artefactValue = false)
    {
        if (!artefactPrefab) artefactPrefab = GameObject.Find("Artifacts");
        GameObject temp = Instantiate(artefactPrefab, Vector3.zero, Quaternion.identity); // initialization of file prefab
        temp.transform.SetParent(artefactsObject.transform.Find("ContentMask").transform.Find("Content").transform); // setting parent of prefab to content
        temp.transform.name = "Artefact" + artefactID;// changing name for individual identification
        temp.transform.localScale = Vector3.one;//normalizing size of prefab
        temp.GetComponent<Artefact>().SetID(artefactID);

        Button button = artefactsObject.transform.Find("ContentMask").transform.Find("Content").transform.Find(temp.transform.name).transform.Find("ButtonRemove").GetComponent<Button>(); //getting button
        button.onClick.AddListener(delegate { temp.GetComponent<Artefact>().DestroyMe(artefacts); SaveLoadBookProperties.SaveArtefacts(bookName, artefacts); }); // setting up destroyer of item connected to button


        InputField inputFieldName = temp.transform.Find("InputFieldArtefactName").GetComponent<InputField>();
        inputFieldName.onEndEdit.AddListener(delegate { temp.GetComponent<Artefact>().SetName(inputFieldName.text); temp.transform.name = inputFieldName.text; RefreshPaths(); SaveLoadBookProperties.SaveArtefacts(bookName, artefacts); }); // saving name of artefact on change
        temp.transform.Find("InputFieldArtefactName").Find("PlaceholderArtefactName").GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "PlaceholderArtefactName", LANGUAGELIBRARY);
        //in case of loading already created artefact appy its properties
        if (artefactName != null)
        {
            temp.GetComponent<Artefact>().name = artefactName;
            inputFieldName.text = artefactName;
            temp.GetComponent<Artefact>().isActive = artefactValue;
            temp.transform.name = artefactName;
        }

        artefacts.Add(temp);

        artefactsObject.transform.Find("ContentMask").transform.Find("Content").transform.Find("ButtonAdd").transform.SetAsLastSibling();// moving adding button to end of groap
        SaveLoadBookProperties.SaveArtefacts(bookName, artefacts);
    }
    public void UpdatePaths()
    {
        DeletePaths();
        SaveLoadPageProperties.LoadPaths(bookName, currentPageIndex);
    }
    public void RefreshPaths()
    {
        foreach (Transform child in pathsObject.transform.Find("ContentMask").transform.Find("Content").transform)
        {
            if (child.name == "ButtonAddPath") continue;
            if (child.name == "BackgroundGrabber") continue;
            foreach (Transform condition in child.transform.Find("Conditions").transform.Find("SkillConditionsArea").transform)
            {
                UpdateDropdownData(condition, "DropdownSkillName", skills, 1);
            }
            foreach (Transform condition in child.transform.Find("Conditions").transform.Find("ArtefactConditionsArea").transform)
            {
                UpdateDropdownData(condition, "DropdownArtefactName", artefacts, 2);
            }
            foreach (Transform effect in child.transform.Find("Effects").transform.Find("SkillEffectsArea").transform)
            {
                UpdateDropdownData(effect, "DropdownSkillName", skills, 1);
            }
            foreach (Transform effect in child.transform.Find("Effects").transform.Find("ArtefactEffectsArea").transform)
            {
                UpdateDropdownData(effect, "DropdownArtefactName", artefacts, 2);
            }
        }
    }
    public bool UpdateDropdownData(Transform dropdownParent, string dropdownName, List<GameObject> listOfNewObjects, int indexForListCreation)
    {
        //if (condition.Find("DropdownSkillName").GetComponent<Dropdown>().options.Count == skills.Count) break;//if number of elements in dropdawn and list of skills are same dont update
        Dropdown dropdownUpdateData = dropdownParent.Find(dropdownName).GetComponent<Dropdown>();
        List<Dropdown.OptionData> newData = GetDropdownDataFromList(listOfNewObjects, indexForListCreation);
        int dropDownValue = dropdownUpdateData.value;
        if (dropdownUpdateData.options[dropDownValue].text != newData[dropDownValue].text) dropDownValue = 0;
        if (newData.Count != dropdownUpdateData.options.Count)
        {
            dropdownUpdateData.ClearOptions();
            dropdownUpdateData.options = newData;
            dropdownUpdateData.value = dropDownValue;
            return true;
        }
        for (int i = 0; i < newData.Count; i++)
        {
            if (dropdownUpdateData.options[i].text != newData[i].text) dropdownUpdateData.options[i].text = newData[i].text;
        }
        dropdownUpdateData.value = dropDownValue;
        return false;
    }
    public void DeletePaths()
    {
        foreach (GameObject path in paths.ToArray())
        {
            Destroy(GameObject.Find(path.GetComponent<MyPath>().connectingFile));
            path.GetComponent<MyPath>().DestroyMe(paths);
        }
        paths.Clear();
    }
    public void AddPath()
    {
        int pathID = paths.Count;
        CreatePath(pathID);

        //LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        //pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        //pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<VerticalLayoutGroup>().SetLayoutVertical();



        //pathsObject.transform.Find("ContentMask").GetComponent<ScrollRect>();
        //pathsObject.transform.Find("ContentMask").transform.Find("Content").transform.position = new Vector3 (0,1,0);

        //Canvas.ForceUpdateCanvases();
        //pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<VerticalLayoutGroup>().enabled = false;
        //pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<VerticalLayoutGroup>().enabled = true;
        UpdateCanvasSorting(pathsObject);
        //float contentHight = pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<RectTransform>().rect.height;
        //float heightOffset = contentHight + 20f;// cca artefact size (little smaller) * number of artefacts + buttonAddHeight
        //pathsObject.transform.Find("ContentMask").transform.Find("Content").transform.Find("Path" + pathID).transform.position = new Vector3(0, -20, 0); // setting up scrolling window at bottom
    }
    public void CreatePath(int pathID, MyPathPrefab myPathPrefab = null, bool isDeathEnd = false, bool isTheEnd = false)
    {
        if (!pathPrefab) pathPrefab = GameObject.Find("StorySetupMenu");
        GameObject temp = Instantiate(pathPrefab, Vector3.zero, Quaternion.identity);// initialization of file prefab
        GameObject tempConditions = temp.transform.Find("Conditions").gameObject;
        GameObject tempEffects = temp.transform.Find("Effects").gameObject;
        temp.transform.SetParent(pathsObject.transform.Find("ContentMask").transform.Find("Content").transform);// setting parent of prefab to content
        temp.transform.name = "Path" + pathID;// changing name for individual identification
        temp.transform.localScale = Vector3.one;// normalizing size of prefab
        temp.GetComponent<MyPath>().SetID(pathID);
        temp.GetComponent<MyPath>().connectingFile = "Page" + currentPageIndex.ToString() + pathID.ToString();
        temp.transform.Find("Head").transform.Find("PathIndex").GetComponent<Text>().text = pathID.ToString();
        temp.transform.Find("Head").transform.Find("ConnectingFile").GetComponent<Text>().text = "Page" + currentPageIndex.ToString() + pathID.ToString();
        //Conditions

        tempConditions.transform.Find("AddingButtons").Find("TextConditions").GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "TextConditions", LANGUAGELIBRARY);
        tempConditions.transform.Find("AddingButtons").Find("ButtonAddSkillCheck").Find("TextAddSkillCheck").GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "TextAddSkillCheck", LANGUAGELIBRARY);
        tempConditions.transform.Find("AddingButtons").Find("ButtonAddArtefactCheck").Find("TextAddArtefactCheck").GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "TextAddArtefactCheck", LANGUAGELIBRARY);

        Button addSkillCheckButton = tempConditions.transform.Find("AddingButtons").Find("ButtonAddSkillCheck").GetComponent<Button>();// getting button
        Button addArtefactCheckButton = tempConditions.transform.Find("AddingButtons").Find("ButtonAddArtefactCheck").GetComponent<Button>();
        Button clearConditionsButton = tempConditions.transform.Find("AddingButtons").Find("ButtonClearConditions").GetComponent<Button>();

        addSkillCheckButton.onClick.AddListener(delegate { AddSkillAction(temp.GetComponent<MyPath>().GetConditionsSkillActions(), temp, "Conditions"); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });// setting up button
        addArtefactCheckButton.onClick.AddListener(delegate { AddArtefactAction(temp.GetComponent<MyPath>().GetConditionsArtefactActions(), temp, "Conditions"); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });
        clearConditionsButton.onClick.AddListener(delegate { temp.GetComponent<MyPath>().ClearConditions(); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });
        //Efects

        tempEffects.transform.Find("AddingButtons").Find("TextEffects").GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "TextEffects", LANGUAGELIBRARY);
        tempEffects.transform.Find("AddingButtons").Find("ButtonAddSkillEffect").Find("TextAddSkillEffect").GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "TextAddSkillEffect", LANGUAGELIBRARY);
        tempEffects.transform.Find("AddingButtons").Find("ButtonAddArtefactEffect").Find("TextAddArtefactEffect").GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "TextAddArtefactEffect", LANGUAGELIBRARY);

        Button addSkillEfectButton = tempEffects.transform.Find("AddingButtons").Find("ButtonAddSkillEffect").GetComponent<Button>();
        Button addArtefactEfectButton = tempEffects.transform.Find("AddingButtons").Find("ButtonAddArtefactEffect").GetComponent<Button>();
        Button clearEfectsButton = tempEffects.transform.Find("AddingButtons").Find("ButtonClearEffects").GetComponent<Button>();

        addSkillEfectButton.onClick.AddListener(delegate { AddSkillAction(temp.GetComponent<MyPath>().GetEffectsSkillActions(), temp, "Effects"); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });// setting up button
        addArtefactEfectButton.onClick.AddListener(delegate { AddArtefactAction(temp.GetComponent<MyPath>().GetEffectsArtefactActions(), temp, "Effects"); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });
        clearEfectsButton.onClick.AddListener(delegate { temp.GetComponent<MyPath>().ClearEffects(); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });

        string connectingFile = "Page" + currentPageIndex.ToString() + pathID.ToString();
        Button createFile = temp.transform.Find("Head").transform.Find("ButtonCreateFile").GetComponent<Button>();
        createFile.onClick.AddListener(delegate { CreateFileForPath(temp.transform.Find("Head").gameObject);
            temp.GetComponent<MyPath>().connectingFile = connectingFile; temp.transform.name = connectingFile; SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });// setting up destroyer of item connected to button
        Button createPageFile = temp.transform.Find("Head").transform.Find("ButtonCreatePageFile").GetComponent<Button>();
        createPageFile.onClick.AddListener(delegate { CreateNewPage(currentPageIndex.ToString() + pathID.ToString()); });// setting up destroyer of item connected to button
        Button selectFile = temp.transform.Find("Head").transform.Find("ButtonSelectCustomFile").GetComponent<Button>();
        selectFile.onClick.AddListener(delegate { SelectFileForPath(temp.transform.Find("Head").gameObject); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });// setting up destroyer of item connected to button

        Button buttonDeletePath = pathsObject.transform.Find("ContentMask").Find("Content").Find(temp.transform.name).Find("Head").Find("ButtonDeletePath").GetComponent<Button>();//getting button
        buttonDeletePath.onClick.AddListener(delegate { temp.GetComponent<MyPath>().DestroyMe(paths); Destroy(GameObject.Find(temp.GetComponent<MyPath>().connectingFile)); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });// setting up destroyer of item connected to button

        if (myPathPrefab != null)
        {
            temp.GetComponent<MyPath>().connectingFile = myPathPrefab.connectingFile;
            temp.GetComponent<MyPath>().storyChoiseText = myPathPrefab.storyChoiseText;
        }
        if (temp.GetComponent<MyPath>().connectingFile != null) temp.transform.Find("Head").Find("ConnectingFile").GetComponent<Text>().text = temp.GetComponent<MyPath>().connectingFile;
        paths.Add(temp);

        CreateStoryPath(temp.GetComponent<MyPath>());

        Toggle deathToggle = temp.transform.Find("Death toggle").Find("Toggle").GetComponent<Toggle>();
        deathToggle.onValueChanged.AddListener(delegate { ChangeDeathToggle(deathToggle, tempConditions, tempEffects); });
        temp.transform.Find("Death toggle").Find("Toggle").Find("LabelDeathEndToggle").GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "LabelDeathEndToggle", LANGUAGELIBRARY);

        deathToggle.isOn = isDeathEnd;

        if (deathToggle.isOn)
        {
            tempConditions.SetActive(false);
            tempEffects.SetActive(false);
            deathToggle.onValueChanged.AddListener(delegate { });
        }

        pathsObject.transform.Find("ContentMask").Find("Content").Find("ButtonAddPath").SetAsLastSibling();// moving adding button to end of groap
        SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths);
    }
    public void ChangeDeathToggle(Toggle deathToggle, GameObject conditions, GameObject effects)
    {
        if (deathToggle.isOn)
        {
            conditions.SetActive(false);
            effects.SetActive(false);
        }
        else
        {
            conditions.SetActive(true);
            effects.SetActive(true);
        }
    }
    public void CreateStoryPath(MyPath connectedPath)
    {
        GameObject storyChoise = Instantiate(storyChoisePrefab, Vector3.zero, Quaternion.identity, GameObject.Find("StoryChoises").transform);
        InputField storyChoiseInputField = storyChoise.transform.Find("StoryChoiseInputField").GetComponent<InputField>();
        storyChoise.transform.Find("StoryChoiseInputField").Find("PlaceholderStoryChoise").GetComponent<Text>().text = LanguageHandler.GetTextTranslation(language, "PlaceholderStoryChoise", LANGUAGELIBRARY);
        //storyChoiseInputField.onValueChanged.AddListener(delegate { test(storyChoise); });
        storyChoiseInputField.onEndEdit.AddListener(delegate { connectedPath.storyChoiseText = storyChoiseInputField.text; });
        storyChoise.transform.name = connectedPath.connectingFile;

        if (connectedPath.storyChoiseText != null)
        {
            storyChoiseInputField.text = connectedPath.storyChoiseText;
            //test(storyChoise);
        }
        storyChoiseInputField.onValueChanged.AddListener(delegate { test(storyChoise); }); // must be after previous canvas update or it will couse error
    }
    public void test(GameObject storyChoise)
    {
        //Canvas.ForceUpdateCanvases();
        storyChoise.GetComponent<ContentSizeFitter>().enabled = false;
        storyChoise.GetComponent<VerticalLayoutGroup>().enabled = false;
        storyChoise.GetComponent<ContentSizeFitter>().enabled = true;
        storyChoise.GetComponent<VerticalLayoutGroup>().enabled = true;

        //Canvas.ForceUpdateCanvases();
        storyChoise.transform.parent.GetComponent<ContentSizeFitter>().enabled = false;
        storyChoise.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
        storyChoise.transform.parent.GetComponent<ContentSizeFitter>().enabled = true;
        storyChoise.transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;
        //Canvas.ForceUpdateCanvases();
    }
    public void CreateFileForPath(GameObject headOfPath)
    {
        if (!headOfPath.transform.Find("ConnectingFile").gameObject.activeSelf)
        {
            headOfPath.transform.Find("ConnectingFile").gameObject.SetActive(true);
            headOfPath.transform.Find("ConnectingFileSelect").gameObject.SetActive(false);
        }
    }
    public void SelectFileForPath(GameObject headOfPath)
    {// dont forget to setup all names correctly
        Dropdown dropdownFileSelect = headOfPath.transform.Find("ConnectingFileSelect").GetComponent<Dropdown>();
        dropdownFileSelect.value = 0;
        if (!headOfPath.transform.Find("ConnectingFileSelect").gameObject.activeSelf)
        {
            headOfPath.transform.Find("ConnectingFile").gameObject.SetActive(false);
            headOfPath.transform.Find("ConnectingFileSelect").gameObject.SetActive(true);
        }
        string[] files = GetStoryFiles();
        List<Dropdown.OptionData> dropdownDataList = new List<Dropdown.OptionData>();
        Dropdown.OptionData fileData = new Dropdown.OptionData();
        fileData.text = "Select file...";
        dropdownDataList.Add(fileData);
        foreach (string file in files)
        {
            string fileName = file.Split('\\')[file.Split('\\').Length - 1].Split('.')[0]; // extracting filename from path
            if (!fileName.StartsWith("page")) continue;

            fileData = new Dropdown.OptionData();
            fileData.text = fileName;
            dropdownDataList.Add(fileData);
        }
        dropdownFileSelect.options.Clear();
        dropdownFileSelect.options = dropdownDataList;
        dropdownFileSelect.onValueChanged.AddListener(delegate { headOfPath.transform.parent.GetComponent<MyPath>().connectingFile = dropdownFileSelect.options[dropdownFileSelect.value].text; });
    }
    public void DeleteArtefactActions(List<GameObject> artefactActions)
    {
        foreach (GameObject artefact in artefactActions.ToArray())
        {
            artefact.GetComponent<Artefact>().DestroyMe(artefactActions);
        }
    }
    public void AddArtefactAction(List<GameObject> artefactActions, GameObject path, string groapName)
    {
        int artefactActionID = artefactActions.Count;
        CreateArtefactAction(artefactActions, path, groapName, artefactActionID);

        UpdateCanvasSorting(path, groapName);
    }
    public void CreateArtefactAction(List<GameObject> artefactActions, GameObject parent, string groapName, int artefactActionID, string artefactName = null, bool isActive = false)
    {
        GameObject temp = Instantiate(artefactAcrionPrefab, Vector3.zero, Quaternion.identity); // initialization of file prefab
        temp.transform.SetParent(parent.transform.Find(groapName).transform.Find("Artefact" + groapName + "Area").transform); // setting parent of prefab to content
        temp.transform.name = "artefactAction" + artefactActionID;// changing name for individual identification
        temp.transform.localScale = Vector3.one;//normalizing size of prefab
        temp.GetComponent<Artefact>().SetID(artefactActionID);
        temp.GetComponent<Artefact>().category = groapName;

        List<Dropdown.OptionData> artefactValData = new List<Dropdown.OptionData>();
        artefactValData.Add(new Dropdown.OptionData(LanguageHandler.GetTextTranslation(language, "ArtefactBooleanTrue", LANGUAGELIBRARY)));
        artefactValData.Add(new Dropdown.OptionData(LanguageHandler.GetTextTranslation(language, "ArtefactBooleanFalse", LANGUAGELIBRARY)));
        Dropdown artefactNameDropdown = temp.transform.Find("DropdownArtefactName").GetComponent<Dropdown>();
        Dropdown artefactValueDropdown = temp.transform.Find("DropdownArtefactValue").GetComponent<Dropdown>();
        artefactNameDropdown.ClearOptions();
        artefactNameDropdown.options = GetDropdownDataFromList(artefacts, 2);
        artefactValueDropdown.options = artefactValData;
        artefactNameDropdown.onValueChanged.AddListener(delegate { temp.GetComponent<Artefact>().SetName(artefactNameDropdown.options[artefactNameDropdown.value].text); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });
        artefactValueDropdown.onValueChanged.AddListener(delegate { temp.GetComponent<Artefact>().SetValue(artefactValueDropdown.value); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });


        //setup parent as parameter
        Button button = temp.transform.Find("ButtonDeleteArtefactAction").GetComponent<Button>(); //getting button
        if (groapName == "Conditions")
        {
            button.onClick.AddListener(delegate { temp.GetComponent<Artefact>().DestroyMe(parent.GetComponent<MyPath>().GetConditionsArtefactActions()); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); }); // setting up destroyer of item connected to button
        }
        else if (groapName == "Effects")
        {
            button.onClick.AddListener(delegate { temp.GetComponent<Artefact>().DestroyMe(parent.GetComponent<MyPath>().GetEffectsArtefactActions()); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); }); // setting up destroyer of item connected to button
        }

        if (artefactName != null)
        {
            temp.GetComponent<Artefact>().name = artefactName;
            temp.GetComponent<Artefact>().isActive= isActive;

            if (isActive) artefactValueDropdown.value = 1;

            foreach (Dropdown.OptionData artefact in artefactNameDropdown.options)
            {
                if (artefact.text == artefactName)
                {
                    artefactNameDropdown.value = artefactNameDropdown.options.IndexOf(artefact);
                    break;
                }
            }
        }

        artefactActions.Add(temp);
        SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths);
    }
    public void DeleteSkillActions(List<GameObject> skillActions)
    {
        foreach (GameObject skillAction in skillActions.ToArray())
        {
            skillAction.GetComponent<Skill>().DestroyMe(skillActions);
        }
    }
    public void AddSkillAction(List<GameObject> skillActions, GameObject path, string groapName)
    {
        int skillActionID = skillActions.Count;
        CreateSkillAction(skillActions, path, groapName, skillActionID);


        Canvas.ForceUpdateCanvases();
        path.transform.Find(groapName).transform.Find("Skill" + groapName + "Area").GetComponent<VerticalLayoutGroup>().enabled = false;
        path.transform.Find(groapName).GetComponent<VerticalLayoutGroup>().enabled = false;
        path.transform.Find(groapName).transform.Find("Skill" + groapName + "Area").GetComponent<VerticalLayoutGroup>().enabled = true;
        path.transform.Find(groapName).GetComponent<VerticalLayoutGroup>().enabled = true;


        Canvas.ForceUpdateCanvases();
        path.transform.Find(groapName).transform.Find("Skill" + groapName + "Area").GetComponent<ContentSizeFitter>().enabled = false;
        path.transform.Find(groapName).transform.Find("Skill" + groapName + "Area").GetComponent<ContentSizeFitter>().enabled = true;

        Canvas.ForceUpdateCanvases();
        pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<VerticalLayoutGroup>().enabled = false;
        pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<VerticalLayoutGroup>().enabled = true;
    }
    public void CreateSkillAction(List<GameObject> skillActions, GameObject path, string groapName, int skillActionID, string skillName = null, int skillValue = 0, int matematicalAction = 0)
    {
        GameObject temp = Instantiate(skillActionPrefab, Vector3.zero, Quaternion.identity); // initialization of file prefab
        temp.transform.SetParent(path.transform.Find(groapName).transform.Find("Skill" + groapName + "Area").transform); // setting parent of prefab to content
        temp.transform.name = "SkillAction" + skillActionID;// changing name for individual identification
        temp.transform.localScale = Vector3.one;//normalizing size of prefab
        temp.GetComponent<Skill>().SetID(skillActionID);
        temp.GetComponent<Skill>().groapName = groapName;

        Dropdown dropdownSkillName = temp.transform.Find("DropdownSkillName").GetComponent<Dropdown>();
        dropdownSkillName.ClearOptions();
        dropdownSkillName.options = GetDropdownDataFromList(skills, 1);
        dropdownSkillName.onValueChanged.AddListener(delegate { temp.GetComponent<Skill>().name = dropdownSkillName.options[dropdownSkillName.value].text; SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });

        InputField valueInputField = temp.transform.Find("InputFieldSkillValue").GetComponent<InputField>();
        valueInputField.onEndEdit.AddListener( delegate { temp.GetComponent<Skill>().value = int.Parse(valueInputField.text); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); } );

        Dropdown skillNameDropdown = temp.transform.Find("DropdownSkillName").GetComponent<Dropdown>();
        Dropdown matematicalActionDropdown = temp.transform.Find("MathematicalAction").GetComponent<Dropdown>();
        List<Dropdown.OptionData> skillValData = new List<Dropdown.OptionData>();
        skillValData.Add(new Dropdown.OptionData(LanguageHandler.GetTextTranslation(language, "MathematicalOperationAddition", LANGUAGELIBRARY)));
        skillValData.Add(new Dropdown.OptionData(LanguageHandler.GetTextTranslation(language, "MathematicalOperationSubtraction", LANGUAGELIBRARY)));
        matematicalActionDropdown.options = skillValData;
        skillNameDropdown.onValueChanged.AddListener(delegate { temp.GetComponent<Skill>().SetName(skillNameDropdown.options[skillNameDropdown.value].text); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });
        matematicalActionDropdown.onValueChanged.AddListener(delegate { temp.GetComponent<Skill>().matematicalOperation = matematicalActionDropdown.value; SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); });



        Button button = temp.transform.Find("ButtonDeleteSkillAction").GetComponent<Button>(); //getting button
        if (groapName == "Conditions")
        {
            button.onClick.AddListener(delegate { temp.GetComponent<Skill>().DestroyMe(path.GetComponent<MyPath>().GetConditionsSkillActions()); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths); }); // setting up destroyer of item connected to button
            matematicalActionDropdown.gameObject.SetActive(false);
        }
        else if (groapName == "Effects")
        {
            button.onClick.AddListener(delegate { temp.GetComponent<Skill>().DestroyMe(path.GetComponent<MyPath>().GetEffectsSkillActions()); SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths);  }); // setting up destroyer of item connected to button
        }

        if (skillName != null)
        {
            temp.GetComponent<Skill>().name = skillName;
            temp.GetComponent<Skill>().value = skillValue;
            temp.GetComponent<Skill>().matematicalOperation = matematicalAction;
            matematicalActionDropdown.value = matematicalAction;
            valueInputField.text = skillValue.ToString();
            foreach (Dropdown.OptionData skill in skillNameDropdown.options)
            {
                if (skill.text == skillName)
                {
                    skillNameDropdown.value = skillNameDropdown.options.IndexOf(skill);
                    break;
                }
            }
        }

        skillActions.Add(temp);
        SaveLoadPageProperties.SavePaths(bookName, currentPageIndex, paths);
    }
    public void UpdateObjectList(List<GameObject> list)
    {
        int i = 0;
        foreach(GameObject go in list)
        {
            if (go.GetComponent<IProperty>() != null)
            {
                go.GetComponent<IProperty>().SetID(i);
                go.transform.name = go.GetComponent<IProperty>().GetTypeName() + i;
            }//if component is porperty like skill or artefact
            else if (go.GetComponent<MyPath>() != null)
            {
                go.GetComponent<MyPath>().SetID(i);
                go.transform.name = go.GetComponent<MyPath>().GetTypeName() + i;
            }// if component is path
            i++;
        }
    }
    public void UpdateCanvasSorting(GameObject path, string groapName = null, int id = -1)
    {
        Canvas.ForceUpdateCanvases();
        pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<VerticalLayoutGroup>().enabled = false;
        pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<ContentSizeFitter>().enabled = false;
        pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<VerticalLayoutGroup>().enabled = true;
        pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<ContentSizeFitter>().enabled = true;
        if (groapName != null)
        {
            Canvas.ForceUpdateCanvases();
            path.transform.Find(groapName).GetComponent<ContentSizeFitter>().enabled = false;
            path.transform.Find(groapName).GetComponent<VerticalLayoutGroup>().enabled = false;
            path.transform.Find(groapName).GetComponent<ContentSizeFitter>().enabled = true;
            path.transform.Find(groapName).GetComponent<VerticalLayoutGroup>().enabled = true;

            Canvas.ForceUpdateCanvases();
            path.transform.Find(groapName).transform.Find("Artefact" + groapName + "Area").GetComponent<VerticalLayoutGroup>().enabled = false;
            path.transform.Find(groapName).transform.Find("Artefact" + groapName + "Area").GetComponent<ContentSizeFitter>().enabled = false;
            path.transform.Find(groapName).transform.Find("Artefact" + groapName + "Area").GetComponent<VerticalLayoutGroup>().enabled = true;
            path.transform.Find(groapName).transform.Find("Artefact" + groapName + "Area").GetComponent<ContentSizeFitter>().enabled = true;
        }



        Canvas.ForceUpdateCanvases();
        pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
        pathsObject.transform.Find("ContentMask").transform.Find("Content").GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
        if (groapName != null)
        {
            path.transform.Find(groapName).GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            path.transform.Find(groapName).GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
            path.transform.Find(groapName).transform.Find("Artefact" + groapName + "Area").GetComponent<VerticalLayoutGroup>().CalculateLayoutInputVertical();
            path.transform.Find(groapName).transform.Find("Artefact" + groapName + "Area").GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
            LayoutRebuilder.MarkLayoutForRebuild(path.transform.Find(groapName).transform.Find("Artefact" + groapName + "Area").transform as RectTransform);
            LayoutRebuilder.MarkLayoutForRebuild(path.transform.Find(groapName).transform as RectTransform);
        }
        Canvas.ForceUpdateCanvases();
    }
    public List<Dropdown.OptionData> GetDropdownDataFromList(List<GameObject> list, int objectTypeID = 0)
    {
        List<Dropdown.OptionData> DropdownDataList = new List<Dropdown.OptionData>();
        Dropdown.OptionData newData;
        switch (objectTypeID)
        {
            case 0:
                newData = new Dropdown.OptionData();
                newData.text = LanguageHandler.GetTextTranslation(language, "DropdownSelect", LANGUAGELIBRARY);
                DropdownDataList.Add(newData);
                break;
            case 1:
                newData = new Dropdown.OptionData();
                newData.text = LanguageHandler.GetTextTranslation(language, "DropdownSelectSkill", LANGUAGELIBRARY);
                DropdownDataList.Add(newData);
                break;
            case 2:
                newData = new Dropdown.OptionData();
                newData.text = LanguageHandler.GetTextTranslation(language, "DropdownSelectArtefact", LANGUAGELIBRARY);
                DropdownDataList.Add(newData);
                break;
            default:
                newData = new Dropdown.OptionData();
                newData.text = LanguageHandler.GetTextTranslation(language, "DropdownSelectDeafult", LANGUAGELIBRARY);
                DropdownDataList.Add(newData);
                break;
        }
        foreach (GameObject obj in list)
        {
            newData = new Dropdown.OptionData();
            newData.text = obj.GetComponent<IProperty>().GetName();

            DropdownDataList.Add(newData);
        }

        return DropdownDataList;
    }
    public void BackToLibrary()
    {
        SaveCurrentPage();
        SceneManager.LoadScene("Library");
    }
    public void DeletePage(string index)
    {
        WarningSetActive(true);
        warningObject.transform.Find("Panel").transform.Find("ButtonYes").GetComponent<Button>().onClick.AddListener(delegate { DeletePageConfirmed(index); });
    }
    public void DeletePageConfirmed(string index)
    {
        warningObject.transform.Find("Panel").transform.Find("ButtonYes").GetComponent<Button>().onClick.RemoveAllListeners();
        if (checkpoints.Contains(index)) checkpoints.Remove(index);
        if (endings.Contains(index)) endings.Remove(index);
        File.Delete(savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + SaveLoadPage.FILENAME + index + SaveLoad.SUFFIX);
        UpdateStoryFiles();
        RefreshPaths();
        WarningSetActive(false);
    }
    public void WarningSetActive(bool isActive)
    {
        warningObject.SetActive(isActive);
    }
    public void CopyStringFromInputFieldToTextObject(GameObject inputField, Text textObject)
    {
        string textToCopy = inputField.GetComponent<InputField>().text;
        textObject.text = textToCopy;
    }
    public void LoadCache()
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
    public void RenamePage(GameObject page, InputField inputFieldName)
    {
        if (page.transform.name == inputFieldName.text || inputFieldName.text.Length < 1)
        {
            inputFieldName.text = page.name;
            return;
        }

        if (bookName[bookName.Length - 1] == ' ')
        {
            Debug.Log("repairing book name bcs of spacebar");
            bookName.Remove(bookName.Length - 2, 1);
        }
        string newName = "page-" + inputFieldName.text;
        string oldName = page.transform.name;
        if (File.Exists(savingPath + newName) || File.Exists(savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + newName + SaveLoad.SUFFIX))
        {
            inputFieldName.text = page.name;
            return;
        }

        if (checkpoints.Contains(oldName))
        {
            checkpoints.Remove(oldName);
            checkpoints.Add(newName.Split('-')[1]);
        }
        if (endings.Contains(oldName))
        {
            endings.Remove(oldName);
            endings.Add(newName.Split('-')[1]);
        }
        File.Move(savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoadPage.FILENAME + page.transform.name + SaveLoad.SUFFIX, savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + newName + SaveLoad.SUFFIX);
        if (currentpageName == page.transform.name)
        {
            Debug.Log("Renaming boi");
            currentPageIndex = inputFieldName.text;
            currentpageName = inputFieldName.text;
        }
        page.transform.name = newName;
        if (!File.Exists(savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + "paths-" + inputFieldName.text + SaveLoad.SUFFIX) &&
        (File.Exists(savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + "paths-" + oldName + SaveLoad.SUFFIX)))
        File.Move(savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + "paths-" + oldName + SaveLoad.SUFFIX, savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + "paths-" + inputFieldName.text + SaveLoad.SUFFIX);
        //SaveCurrentPage();
        UpdateStoryFiles();
    }
    public void SetupCheckpoint()
    {
        if (checkpoints.Contains(currentpageName))
        {
            checkpointObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            checkpointObject.GetComponent<Toggle>().isOn = true;
            checkpointObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RemoveCheckpoint(currentpageName); });
        }
        else 
        { 
            checkpointObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            checkpointObject.GetComponent<Toggle>().isOn = false;
            checkpointObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate { AddCheckpoint(currentpageName); });
        }
    }
    public void SetupEndig()
    {
        if (endings.Contains(currentpageName))
        {
            endingObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            endingObject.GetComponent<Toggle>().isOn = true;
            endingObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RemoveEndig(currentpageName); });
        }
        else
        {
            endingObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            endingObject.GetComponent<Toggle>().isOn = false;
            endingObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate { AddEnding(currentpageName); });
        }
    }
    public void UpdateCheckpoint()
    {
        if (checkpointObject.GetComponent<Toggle>().isOn)
        {
            checkpointObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            checkpointObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RemoveCheckpoint(currentpageName); });
        }
        else
        {
            checkpointObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            checkpointObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate { AddCheckpoint(currentpageName); });
        }
    }
    public void UpdateEnding()
    {
        if (endingObject.GetComponent<Toggle>().isOn)
        {
            endingObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            endingObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate { RemoveEndig(currentpageName); });
        }
        else
        {
            endingObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            endingObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate { AddEnding(currentpageName); });
        }
    }
    public void LoadCheckpoints()
    {
        string checkpointSavingPath = savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + CHECKPOINTFILENAME;
        if (!File.Exists(checkpointSavingPath)) return;
        checkpoints.Clear();
        string loadedCheckpoints = File.ReadAllText(checkpointSavingPath);
        foreach (string checkpoint in loadedCheckpoints.Split(SaveLoadBookProperties.SEPARATOR))
        {
            if (checkpoint.Length > 2) checkpoints.Add(checkpoint);
        }
    }
    public void LoadEndings()
    {
        string endingSavingPath = savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + ENDINGFILENAME;
        if (!File.Exists(endingSavingPath)) return;
        endings.Clear();
        string loadedEndings = File.ReadAllText(endingSavingPath);
        foreach (string ending in loadedEndings.Split(SaveLoadBookProperties.SEPARATOR))
        {
            if (ending.Length > 2) endings.Add(ending);
        }
    }
    public void SaveCheckpoints()
    {
        string checkpointSavingPath = savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + CHECKPOINTFILENAME;
        string textToSave = "";
        foreach (string checkpoint in checkpoints)
        {
            if (textToSave.Length > 1) textToSave += SaveLoadBookProperties.SEPARATOR;
            textToSave += checkpoint;
        }
        File.WriteAllText(checkpointSavingPath, textToSave);
    }
    public void SaveEndings()
    {
        string endingSavingPath = savingPath + SaveLoad.BOOKPREFIX + bookName + SaveLoad.BOOKPREFIX + ENDINGFILENAME;
        string textToSave = "";
        foreach (string ending in endings)
        {
            if (textToSave.Length > 1) textToSave += SaveLoadBookProperties.SEPARATOR;
            textToSave += ending;
        }
        File.WriteAllText(endingSavingPath, textToSave);
    }
    public void AddCheckpoint(string checkpoint)
    {
        checkpoints.Add(checkpoint);
        SaveCheckpoints();
        UpdateCheckpoint();
    }
    public void RemoveCheckpoint(string checkpoint)
    {
        checkpoints.Remove(checkpoint);
        SaveCheckpoints();
        UpdateCheckpoint();
    }
    public void AddEnding(string ending)
    {
        endings.Add(ending);
        SaveEndings();
        UpdateEnding();
    }
    public void RemoveEndig(string ending)
    {
        endings.Remove(ending);
        SaveEndings();
        UpdateEnding();
    }
}
