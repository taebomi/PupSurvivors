using System;
using System.Collections;
using System.Collections.Generic;
using QFSW.QC;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;


public enum AniType
{
    Idle = 0,
    Move = 1,
    Attack = 4,
    Die = 9,
}

public static class TaeBoMiCache
{
    #region Constant

    public const int WidthPerDot = 5;
    public const float DotPerWidth = 1f / WidthPerDot;

    #endregion
    

    #region 애니메이션

    public static readonly int IsRight = Animator.StringToHash(nameof(IsRight));
    public static readonly int IsMoving = Animator.StringToHash(nameof(IsMoving));
    public static readonly int IsSelected = Animator.StringToHash(nameof(IsSelected));

    public static readonly int AttackTrigger = Animator.StringToHash(nameof(AttackTrigger));
    public static readonly int CriticalTrigger = Animator.StringToHash(nameof(CriticalTrigger));
    public static readonly int DieTrigger = Animator.StringToHash(nameof(DieTrigger));
    public static readonly int EyeBlinkTrigger = Animator.StringToHash(nameof(EyeBlinkTrigger));

    public static readonly int AniType = Animator.StringToHash(nameof(AniType));
    public static readonly int FaceType = Animator.StringToHash(nameof(FaceType));
    public static readonly int AttackCount = Animator.StringToHash(nameof(AttackCount));

    public static readonly int AttackRatio = Animator.StringToHash(nameof(AttackRatio));

    public static readonly int Open = Animator.StringToHash(nameof(Open));
    public static readonly int Close = Animator.StringToHash(nameof(Close));
    public static readonly int Burn = Animator.StringToHash(nameof(Burn));

    #endregion

    #region 레이어

    public static readonly int DamagableLayerMask = LayerMask.GetMask("Damagable", "GroundEnemy", "AirEnemy");

    [Flags]
    public enum LayerName
    {
        Ground,
        Wall,
        GroundEnemy,
        AirEnemy,
        Event,
    }

    private static readonly Dictionary<LayerName, int> NameToLayerDict = new();
    private static readonly Dictionary<LayerName, LayerMask> LayerMaskDict = new();

    public static int GetNameToLayer(LayerName layerName)
    {
        if (!NameToLayerDict.TryGetValue(layerName, out var layerInt))
        {
            NameToLayerDict.Add(layerName,
                layerInt = LayerMask.NameToLayer(Enum.GetName(typeof(LayerName), layerName)));
        }

        return layerInt;
    }

    public static LayerMask GetLayerMask(LayerName layerName)
    {
        if (!LayerMaskDict.TryGetValue(layerName, out var layerMask))
        {
            LayerMaskDict.Add(layerName,
                layerMask = LayerMask.GetMask(Enum.GetName(typeof(LayerName), layerName)));
        }

        return layerMask;
    }

    #endregion

    #region 셰이더

    public static readonly int ColorRampLuminosity = Shader.PropertyToID("_ColorRampLuminosity");

    #endregion

    #region Misc

    [Flags]
    public enum Direction
    {
        None = 0,
        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        UpLeft = Up | Left,
        UpRight = Up | Right,
        DownLeft = Down | Left,
        DownRight = Down | Right,
        Cardinal = Up | Down | Left | Right,
    }

    public static readonly Direction[] Directions =
    {
        Direction.Up, Direction.Down, Direction.Left, Direction.Right
    };



    public static readonly RaycastHit2D[] RaycastHitArr = new RaycastHit2D[100];
    public static readonly Collider2D[] ColliderArr = new Collider2D[100];

    public static ContactFilter2D GroundFilter = new()
        { layerMask = LayerMask.GetMask("Ground", "Wall"), useLayerMask = true, };

    public static ContactFilter2D PlayerFilter = new()
    {
        layerMask = LayerMask.GetMask($"Player"), useLayerMask = true
    };

    public static ContactFilter2D DamagableFilter = new()
        { layerMask = LayerMask.GetMask("AirEnemy", "GroundEnemy"), useLayerMask = true, };

    #endregion
}