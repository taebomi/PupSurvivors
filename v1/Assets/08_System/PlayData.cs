using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PlayData
{
    public string selectedCharacterName;
    public Language language;
    public Dictionary<string, CharacterData> CharacterDataDict;

    public enum Language
    {
        Kor,
        Eng,
    }
    public PlayData()
    {
        selectedCharacterName = "Baekgu";
    }
}
