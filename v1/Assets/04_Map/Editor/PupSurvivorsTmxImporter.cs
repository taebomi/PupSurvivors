#if UNITY_EDITOR
using System.Linq;
using PupSurvivors.Stage.Map;
using SuperTiled2Unity;
using SuperTiled2Unity.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[AutoCustomTmxImporter]
public class PupSurvivorsTmxImporter : CustomTmxImporter
{
    private TmxAssetImportedArgs _args;

    public override void TmxAssetImported(TmxAssetImportedArgs args)
    {
        _args = args;
        
        InitMap();
        InitTilemapRenderer();
        InitGroups();

        var map = args.ImportedSuperMap;
        

        if (map.TryGetComponent<SuperCustomProperties>(out var properties))
        {
            Vector3 pos = Vector3.zero;
            if (properties.TryGetCustomProperty("x", out var x))
            {
                pos.x = (int.Parse(x.m_Value) - 10) * 96;
            }

            if (properties.TryGetCustomProperty("y", out var y))
            {
                pos.y = (int.Parse(y.m_Value) + 1) * 96;
            }

            map.transform.position = pos;
        }
    }

    private void InitMap()
    {
        var superMap = _args.ImportedSuperMap;

        superMap.gameObject.layer = LayerMask.NameToLayer(TaeBoMiCache.LayerName.Event.ToString());
        
        // 맵보다 크게 콜라이더 생성하여 플레이어 근접 시 활성화 용도
        var collider = superMap.gameObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(96 +TempPathFinder.ColumnSize, 96 + TempPathFinder.RowSize);
        collider.offset = new Vector2(48, -48);
        collider.isTrigger = true;
        
        var map =  superMap.gameObject.AddComponent<Map>();
        
        // 실내 체크용
        var mapProperties = superMap.GetComponent<SuperCustomProperties>();
        if (mapProperties.TryGetCustomProperty("IsIndoor", out var isIndoor))
        {
            if (isIndoor.m_Value == "true")
            {
                map.isIndoor = true;
                map.Activate(false);
            }
        }
    }

    private void InitTilemapRenderer()
    {
        var tilemapRenderers = _args.ImportedSuperMap.GetComponentsInChildren<TilemapRenderer>();
        foreach (var tilemapRenderer in tilemapRenderers)
        {
            tilemapRenderer.mode = TilemapRenderer.Mode.Chunk;
            
            // 마스크 세팅
            if (tilemapRenderer.sortingLayerName is "Top")
            {
                tilemapRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
            }
        }
    }

    // todo 다시 수정하기
    private void InitGroups()
    {
        var groups = _args.ImportedSuperMap.GetComponentsInChildren<SuperGroupLayer>();
        
        foreach (var superGroupLayer in groups)
        {
            var groupProperties = superGroupLayer.GetComponent<SuperCustomProperties>();
            if(groupProperties.TryGetCustomProperty("IsComposite", out var groupIsComposite))
            {
                if (groupIsComposite.m_Value == "false")
                {
                    continue;
                }
                
                var colliders = groupProperties.GetComponentsInChildren<Collider2D>();
                foreach (var collider2D in colliders)
                {
                    collider2D.usedByComposite = true;
                }
                
                var groupCompositeCollider = superGroupLayer.gameObject.AddComponent<CompositeCollider2D>();
                groupCompositeCollider.attachedRigidbody.bodyType = RigidbodyType2D.Static;
                groupCompositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;

            }
        }
    }
}
#endif