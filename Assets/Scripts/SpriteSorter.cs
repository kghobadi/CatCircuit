using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Contains methods for sorting sprites in a scene based on Y position. 
/// </summary>
public class SpriteSorter : MonoBehaviour
{
    [SerializeField] private bool sortOnStart;
    [SerializeField] private List<string> layersToSort = new List<string>();
    
    void Start()
    {
        if(sortOnStart)
            SortSpriteRenderers();
    }

    private void Update()
    {
        SortSpriteRenderers();
    }

    void SortSpriteRenderers()
    {
        // Get all SpriteRenderer components in the scene
        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer>();

        // Sort SpriteRenderers by their Y position
        var sortedRenderers = spriteRenderers
            .OrderBy(renderer => renderer.transform.position.y)
            .ToList();

        // Option 1: Adjust Sorting Order
        int order = 0;
        for (int i = sortedRenderers.Count - 1; i > 0; i--)
        {
            if (layersToSort.Contains(sortedRenderers[i].sortingLayerName))
            {
                sortedRenderers[i].sortingOrder = order;  // Set sorting order based on their rank
                order++;
            }
        }

        // Option 2: Adjust Hierarchy (optional)
        // If you want to parent them to a specific transform
        // Transform parentTransform = new GameObject("SortedSprites").transform; 
        // foreach (var renderer in sortedRenderers)
        // {
        //     renderer.transform.SetParent(parentTransform);
        // }
    }
}