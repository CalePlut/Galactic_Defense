using UnityEngine;
using System.Collections;
public class EliasBitMaskAttribute : PropertyAttribute
{
    public System.Type propType;
    public EliasBitMaskAttribute(System.Type aType)
    {
        propType = aType;
    }
}