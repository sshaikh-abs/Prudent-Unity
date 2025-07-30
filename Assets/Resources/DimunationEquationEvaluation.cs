using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimunationEquationEvaluation : MonoBehaviour
{
    public float originalThickness = 12f;
    [Range(0, 12f)] public float measuredThickness = 12f;
    public float result = 0f;

    private void OnValidate()
    {
        result = ((originalThickness - measuredThickness) * 100f) / (originalThickness * 0.2f);
    }
}
