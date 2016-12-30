using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class consumable : Product
{
    public enum consumeanimation { generic, drink_straw, lick, with_hands }
    [SerializeField]
    public consumeanimation ConsumeAnimation;
    public enum temprature { none, cold, hot }
    [SerializeField]
    public temprature Temprature;
    [SerializeField]
    public int portions;
    public override void DrawGUI()
    {

#if UNITY_EDITOR
        base.DrawGUI();
        Hand = hand.Right;
        ConsumeAnimation = (consumeanimation)EditorGUILayout.EnumPopup("Consume Animation ", ConsumeAnimation);
        Temprature = (temprature)EditorGUILayout.EnumPopup("Temprature ", Temprature);
        portions = EditorGUILayout.IntField("Portions ", portions);
        DrawIngredientsGUI();
#endif
    }
}