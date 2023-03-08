using System.Collections;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class LanguageHandler
{
    public static void LanguageLoader(string language, string languageFolder)
    {
        XElement lanuguages = XElement.Load(@"Assets\scripts\Language\" + languageFolder);
        IEnumerable<XElement> level1Elements = lanuguages.Element(language).Elements("string");

        //Debug.Log(lanuguages.Element("English").Element("string").Attribute("name").Value);
        foreach(XElement level2Element in level1Elements)
        {
            //Debug.Log(level2Element.Attribute("name").Value);
            ApplyTextFromLenguage(level2Element.Attribute("name").Value, level2Element.Value);
        }
    }
    private static void ApplyTextFromLenguage(string objectName, string objectText)
    {
        if (!GameObject.Find(objectName)) return;
        GameObject.Find(objectName).GetComponent<Text>().text = objectText;
    }
    public static string GetTextTranslation(string language, string text, string languageFolder)
    {
        XElement lanuguages = XElement.Load(@"Assets\scripts\Language\" + languageFolder);
        IEnumerable<XElement> level1Elements = lanuguages.Element(language).Elements("string");

        foreach (XElement level2Element in level1Elements)
        {
            if (text == level2Element.Attribute("name").Value) return level2Element.Value;
        }
        return null;
    }
}
