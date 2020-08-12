using UnityEngine;

[CreateAssetMenu(fileName = "AffectVariables", menuName = "Balance/affectVariables")]
public class AffectVariables : ScriptableObject
{
    [Header("Reset Speed")]
    [Range(0, 1.0f)]
    public float resetSpeed;

    [Header("Thresholds for emotional changes")]
    public float changeThreshold = 5.0f;

    public float strongThreshold = 10.0f;

    [Header("Mood Variables")]
    [Header("Stage 1")]
    public float V11;

    public float A11, T11;

    [Header("Stage 2")]
    public float V21;

    public float A21, T21;

    [Space]
    public float V22;

    public float A22, T22;

    [Header("Stage 3")]
    public float V31;

    public float A31, T31;

    [Space]
    public float V32;

    public float A32, T32;

    [Space]
    public float V33;

    public float A33, T33;
}