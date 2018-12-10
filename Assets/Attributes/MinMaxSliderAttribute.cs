#if UNITY_EDITOR
using UnityEngine;
public class MinMaxSliderAttribute : PropertyAttribute
{
    public readonly float max;
    public readonly float min;

    public MinMaxSliderAttribute( float min, float max )
    {
        this.min = min;
        this.max = max;
    }
    public MinMaxSliderAttribute( int min, int max )
    {
        this.min = min;
        this.max = max;
    }
}
#endif