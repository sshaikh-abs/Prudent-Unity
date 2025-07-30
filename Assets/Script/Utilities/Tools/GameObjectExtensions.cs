using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    public static GameObject FindChildWithSubstringInName(this GameObject root, string subString)
    {
        foreach (Transform child in root.transform)
        {
            if (child.name.Contains(subString))
            {
                return child.gameObject;
            }

            GameObject found = FindChildWithSubstringInName(child.gameObject, subString);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    public static GameObject FindAncestorByName(this GameObject obj, string targetName)
    {
        Transform current = obj.transform;

        while (current != null)
        {
            if (current.name == targetName)
            {
                return current.gameObject;
            }
            current = current.parent;
        }

        return null;
    }

    public static List<GameObject> GetAllInHierarchy(GameObject root)
    {
        List<GameObject> allObjects = new List<GameObject>();

        AddChildrenRecursively(root, allObjects);

        Transform current = root.transform.parent;
        while (current != null)
        {
            allObjects.Add(current.gameObject);
            current = current.parent;
        }

        return allObjects;
    }

    private static void AddChildrenRecursively(GameObject obj, List<GameObject> list)
    {
        list.Add(obj);

        foreach (Transform child in obj.transform)
        {
            AddChildrenRecursively(child.gameObject, list);
        }
    }
}