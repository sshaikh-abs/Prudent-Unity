using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Expectation", menuName = "ScriptableObjects/Relation Builder/Expectation", order = 1)]
public class Expectation : ScriptableObject
{
    public List<KeyPair> expectations;

    public bool AreExpectationsMet(GameObject gameObject)
    {
        foreach (KeyPair expectation in expectations)
        {
            if(!gameObject.GetComponent<MetadataComponent>().ContainsKey(expectation.key))
            {
                return false;
            }

            if(expectation.shouldValueMatch)
            {
                if (gameObject.GetComponent<MetadataComponent>().GetValue(expectation.key) != expectation.value)
                {
                    return false;
                }
            }

            if (expectation.shouldBeOnOftheLookUpValues)
            {
                var value = gameObject.GetComponent<MetadataComponent>().GetValue(expectation.key);

                if (!expectation.lookUpValues.Contains(value.ToLower()))
                {
                    return false;
                }
            }
        }
        return true;
    }
}

[System.Serializable]
public class KeyPair
{
    public string key;
    public string value;
    public bool shouldValueMatch;
    public bool shouldBeOnOftheLookUpValues;
    public List<string> lookUpValues;
}
