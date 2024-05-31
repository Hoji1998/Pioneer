using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TextData", order = 1)]
public class TextData : ScriptableObject
{
    [Header("TextData Component")]
    public string text;
}
