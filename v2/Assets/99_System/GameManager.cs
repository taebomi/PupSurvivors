using PupSurvivors.System;
using UnityEngine;

public class GameManager : UniqueSingleton<GameManager>
{
    [field: SerializeField] public SaveData SaveData { get; private set; }
    [field: SerializeField] public SettingData SettingData { get; private set; }
    
    [field: SerializeField] public PlayerData[] PlayerData { get; private set; }

    protected override void Initialize()
    {
        SaveData = ES3.Load("SaveData", new SaveData()
        {
            maxWeaponNum = 3,
            maxAccessoryNum = 2,
        });
        SettingData = ES3.Load("SettingData", new SettingData());
    }
}