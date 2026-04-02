using UnityEngine.Events;
using NotImplementedException = System.NotImplementedException;

namespace PupSurvivors.Stage
{
    public enum PlayerState
    {
        Normal,
        Skill,
        Event,
        Die,
    }

    public abstract class PlayerStateBase
    {
        protected readonly Player Player;

        public abstract PlayerState ThisEnum { get; }

        public UnityAction OnStateEnter, OnUpdate, OnFixedUpdate, OnLateUpdate, OnStateExit, OnActButtonDown;


        public PlayerStateBase(Player player)
        {
            Player = player;
        }
    }
}