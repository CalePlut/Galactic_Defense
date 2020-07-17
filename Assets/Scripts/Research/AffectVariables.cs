using UnityEngine;

[CreateAssetMenu(fileName = "AffectVariables", menuName = "Balance/affectVariables")]
public class AffectVariables : ScriptableObject
{
    [Header("Reset Speed")]
    [Range(0, 1.0f)]
    public float resetSpeed;

    [Header("Emotion strength levels")]
    public float weakValence = 15;
    public float moderateValence = 30, strongValence = 45;
    public float weakTension = 5, moderateTension = 10, strongTension = 15;
    public float arousalScalar = 2.0f;

    [Header("Mood Variables")]
    [Header("Stage 1")]
    public float V11;
    public float A11, T11;
    [Space]
    public float V12;
    public float A12, T12;
    [Header("Stage 2")]
    public float V21;
    public float A21, T21;
    [Space]
    public float V22;
    public float A22, T22;
    [Space]
    public float V23;
    public float A23, T23;
    [Header("Stage 3")]
    public float V31;
    public float A31, T31;
    [Space]
    public float V32;
    public float A32, T32;
    [Space]
    public float V33;
    public float A33, T33;
    [Space]
    public float V34;
    public float A34, T34;


}
