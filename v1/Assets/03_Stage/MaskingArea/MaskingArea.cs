using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskingArea : MonoBehaviour
{
    private static int _maskedNum;

    public void SetXSize(float x)
    {
        var bc = GetComponent<BoxCollider2D>();
        bc.size = new Vector2(x, bc.size.y);
        transform.position = new Vector3(transform.position.x + x * 0.5f, transform.position.y, 0f);
    }
    
    public void SetYSize(float y)
    {
        var bc = GetComponent<BoxCollider2D>();
        bc.size = new Vector2(bc.size.x, y);
        transform.position = new Vector3(transform.position.x, transform.position.y - y * 0.5f, 0f);
    }
    private void Awake()
    {
        _maskedNum = 0;
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        _maskedNum++;
        if (_maskedNum >= 1)
        {
            PlayerController.Instance.Mask.ActivateMask(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        _maskedNum--;
        if (_maskedNum <= 0)
        {
            PlayerController.Instance.Mask.ActivateMask(false);
        }
    }
}
