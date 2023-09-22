using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperFunctions
{
    // Start is called before the first frame update
    public static GameObject FindObjectWithTag(Transform currentTransform, string tag)
    {
        // Check if the currentTransform's GameObject has the specified tag.
        if (currentTransform.CompareTag(tag))
        {
            return currentTransform.gameObject;
        }

        // Iterate through the child transforms.
        for (int i = 0; i < currentTransform.childCount; i++)
        {
            Transform child = currentTransform.GetChild(i);

            // Recursively search through the child's hierarchy.
            GameObject foundObject = FindObjectWithTag(child, tag);

            // If a matching object is found in the child's hierarchy, return it.
            if (foundObject != null)
            {
                return foundObject;
            }
        }

        // If no matching object is found in the hierarchy, return null.
        return null;
    }
}
