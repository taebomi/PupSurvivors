using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class TaeBoMiCache
{
    public const int WidthPerDot = 5;
    public const float DotPerWidth = 0.2f;
    
    
    public static readonly int IsMoving = Animator.StringToHash("IsMoving");
    public static readonly int IsRight = Animator.StringToHash("IsRight");
    public static readonly int IsSelected = Animator.StringToHash(nameof(IsSelected));

    public static readonly int Type = Animator.StringToHash(nameof(Type));

    public static readonly int AttackTrigger = Animator.StringToHash(nameof(AttackTrigger));
    public static readonly int MoveTrigger = Animator.StringToHash(nameof(MoveTrigger));
    public static readonly int EyeBlinkTrigger = Animator.StringToHash(nameof(EyeBlinkTrigger));
    public static readonly int DieTrigger = Animator.StringToHash(nameof(DieTrigger));

    public static readonly int AttackRatio = Animator.StringToHash(nameof(AttackRatio));

    public static readonly int Close = Animator.StringToHash(nameof(Close));
    public static readonly int Burn = Animator.StringToHash(nameof(Burn));

    public static readonly int Attack = Animator.StringToHash(nameof(Attack));
    public static readonly int Critical = Animator.StringToHash(nameof(Critical));
    public static readonly int Casting = Animator.StringToHash(nameof(Casting));
    public static readonly int Move = Animator.StringToHash(nameof(Move));
    public static readonly int Die = Animator.StringToHash(nameof(Die));
    public static readonly int FaceState = Animator.StringToHash(nameof(FaceState));
    public enum FaceStateType
    {
        Normal=0,
        Cute=10,
        Pain=100,
    }



    #region 셰이더

    public static readonly int ShineLocation = Shader.PropertyToID("_ShineLocation");
    public static readonly int ColorRampLuminosity = Shader.PropertyToID("_ColorRampLuminosity");
    public static readonly int FadeAmount = Shader.PropertyToID("_FadeAmount");
    public static readonly int Glow = Shader.PropertyToID("_Glow");

    #endregion

    public static class SortingLayer
    {

        public static readonly int Object = UnityEngine.SortingLayer.NameToID(nameof(Object));

    }


    #region 레이어 마스크

    [Flags]
    public enum LayerName
    {
        GroundEnemy,
        AirEnemy,
        Ground,
        Wall,
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

    public static readonly List<Collider2D> TempColliderList = new(200);
}
