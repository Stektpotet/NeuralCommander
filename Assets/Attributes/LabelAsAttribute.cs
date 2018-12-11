
using UnityEngine;
public class LabelAsAttribute : PropertyAttribute
{
    public readonly string text;
    public LabelAsAttribute( string text ) { this.text = text; }
}