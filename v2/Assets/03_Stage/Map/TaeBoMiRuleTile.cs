using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "PupSurvivors/Map/RuleTile", fileName = "RuleTile", order = 100)]
public class TaeBoMiRuleTile : RuleTile<TaeBoMiRuleTile.Neighbor>
{
    public TileBase[] tilesToConnect;

    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int Specified = 10;
        public const int ThisOrSpecified = 11;
        public const int Nothing = 20;
        public const int Any = 30;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        switch (neighbor)
        {
            case TilingRuleOutput.Neighbor.This: return IsThis(tile);
            case Neighbor.NotThis: return !IsThis(tile);
            case Neighbor.Any: return IsAny(tile);
            case Neighbor.Specified: return IsSpecified(tile);
            case Neighbor.ThisOrSpecified: return IsThis(tile) || IsSpecified(tile);
            case Neighbor.Nothing: return IsNothing(tile);
        }

        return base.RuleMatch(neighbor, tile);
    }

    private bool IsThis(TileBase tile)
    {
        return tile == this;
    }

    private bool IsAny(TileBase tile)
    {
        return tile != null;
    }

    private bool IsSpecified(TileBase tile)
    {
        return tilesToConnect.Contains(tile);
    }

    private bool IsNothing(TileBase tile) => tile == null;
}