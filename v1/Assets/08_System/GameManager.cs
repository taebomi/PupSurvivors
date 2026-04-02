using System;
using System.Collections;
using System.Collections.Generic;
using Febucci.UI;
using Febucci.UI.Core;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [field:SerializeField] public PlayData PlayData { get; private set; }

    protected override void AwakeAfter()
    {
        PlayData = ES3.Load("PlayData", new PlayData());
        
        TAnimBuilder.InitializeGlobalDatabase();
    }
}