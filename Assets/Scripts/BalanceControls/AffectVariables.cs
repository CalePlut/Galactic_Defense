using UnityEngine;

[CreateAssetMenu(fileName = "AffectVariables", menuName = "Balance/affectVariables")]
public class AffectVariables : ScriptableObject
{
    [Header("Thresholds for emotional changes")]
    public float positiveThreshold = 15.0f;

    public float negativeThreshold = -15.0f;
    public float strongThreshold = 45f;
    public float strongNegativeThreshold = -45f;

    public Vector3 stage1Mood, stage2Mood, stage3Mood; 

    [Header("Event values: Valence, Tension (no arousal)")]
    public Vector2 P_attack, P_heavy, E_attack, E_heavy, P_shield_down, E_shield_down, P_exploit, P_death, E_death, P_heal, E_heal;


}