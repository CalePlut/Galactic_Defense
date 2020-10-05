using UnityEngine;

[CreateAssetMenu(fileName = "AffectVariables", menuName = "Balance/affectVariables")]
public class AffectVariables : ScriptableObject
{
    [Header("Thresholds for emotional changes")]
    public float positiveThreshold = 1.5f;

    public float negativeThreshold = -1.0f;
    public float strongThreshold = 2.5f;
    public float strongNegativeThreshold = -2f;
}