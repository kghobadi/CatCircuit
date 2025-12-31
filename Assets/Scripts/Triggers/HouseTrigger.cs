using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simply lets house know who is present. 
/// </summary>
public class HouseTrigger : MonoBehaviour
{
    [SerializeField] private SpriteRenderer zoneRenderer;
    public SpriteRenderer HouseZone => zoneRenderer; // only truly get speed boost if enabled? 
    [SerializeField] private House house;

    [SerializeField] private Vector2 maxZoneScale = new Vector2(2.5f, 2f);
    public Vector2 ZoneScaleMax => maxZoneScale;

    private void Start()
    {
        zoneRenderer.enabled = false;
    }

    /// <summary>
    /// Updates the zone scale according to alignment. 
    /// </summary>
    /// <param name="align"></param>
    /// <param name="max"></param>
    public void UpdateZoneScale(float align, float max)
    {
        float alignFillAmt = align / max;
        float xTotal = maxZoneScale.x - 1;
        float xScale = 1 + alignFillAmt * xTotal;
        float yTotal = maxZoneScale.y - 1;
        float yScale = 1 + alignFillAmt * yTotal;
        zoneRenderer.transform.localScale = new Vector3(xScale, yScale,1f);
        if (xScale >= 2)
        {
            zoneRenderer.enabled = true;
        }
        else
        {
            zoneRenderer.enabled = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CatController cat = other.gameObject.GetComponent<CatController>();
            if (cat)
            {
                house.SetPlayerPresent(cat.PlayerID, true);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CatController cat = other.gameObject.GetComponent<CatController>();
            if (cat)
            {
                house.SetPlayerPresent(cat.PlayerID, false);
            }
        }
    }
}
