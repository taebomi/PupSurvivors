using UnityEngine;

namespace PupSurvivors.Stage
{
    public partial class StageManager
    {
        [Header("Player")] [SerializeField] private Player playerPrefab;
        [SerializeField] private Transform playerContainer;
        public Player[] CurPlayers { get; private set; }

        public bool IsSoloPlay => CurPlayers.Length == 1;

        public void InitPlayer()
        {
            var curPlayerData = GameManager.Instance.PlayerData;
            var debugPlayers = GameObject.FindGameObjectsWithTag("Player");
            if (debugPlayers != null) // 미리 배치되어 있는 경우
            {
                CurPlayers = new Player[debugPlayers.Length];
                for (var i = 0; i < debugPlayers.Length; i++)
                {
                    CurPlayers[i] = debugPlayers[i].GetComponent<Player>();
                }
            }
            else
            {
                CurPlayers = new Player[curPlayerData.Length];
                for (var i = 0; i < curPlayerData.Length; i++)
                {
                    CurPlayers[i] = Instantiate(playerPrefab, playerContainer);
                }

                switch (CurPlayers.Length)
                {
                    case 1:
                        CurPlayers[0].transform.position = Vector3.zero;
                        break;
                    case 2:
                        CurPlayers[0].transform.position = new Vector3(-1f, 0f);
                        CurPlayers[1].transform.position = new Vector3(1f, 0f);
                        break;
                    case 3:
                        CurPlayers[0].transform.position = new Vector3(-0.75f, 0.75f);
                        CurPlayers[1].transform.position = new Vector3(0.75f, -0.75f);
                        CurPlayers[2].transform.position = new Vector3(0f, 1f);
                        break;
                    case 4:
                        CurPlayers[0].transform.position = new Vector3(-1f, 1f);
                        CurPlayers[1].transform.position = new Vector3(1f, 1f);
                        CurPlayers[2].transform.position = new Vector3(1f, -1f);
                        CurPlayers[3].transform.position = new Vector3(-1f, -1f);
                        break;
                }
            }

            for (var i = 0; i < CurPlayers.Length; i++)
            {
                _initTaskList.Add(CurPlayers[i].Initialize(i, curPlayerData[i].characterName));
            }
        }
    }
}