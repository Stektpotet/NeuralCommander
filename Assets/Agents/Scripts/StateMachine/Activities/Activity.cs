using System.Collections;
using UnityEngine;


[RequireComponent(typeof(SquadUnit))]
public abstract class Activity : MonoBehaviour
{
    private SquadUnit _agent;
    public SquadUnit Agent => _agent;

    private CommanderAgent _commander;
    public CommanderAgent Commander => _commander;

#if UNITY_EDITOR
    private void Reset()
    {
        Debug.Assert(transform.parent.GetComponent<CommanderAgent>() != null, "Squad agent must be a child of an object with a Commander Agent");
    }
#endif

    private void Start()
    {
        _agent = GetComponent<SquadUnit>();
        _commander = transform.parent.GetComponent<CommanderAgent>();
    }

    [Tooltip("The amount of time to wait when calling Wait() on this activity")]
    [SerializeField] protected float waitTime = 1;
    private float waitTimer = 0;

    /// <summary>
    /// Wait for the time specified by <see cref="waitTime"/>
    /// </summary>
    /// <returns>true while waiting, false when done</returns>
    public bool Wait()
    {
        Agent.Actions.Crouch(Agent.Sensor.foot, Agent.Sensor.OriginalHeight, 0.3f);
        if (waitTimer > waitTime)
        {
            waitTimer = 0;
            return false;
        }
        waitTimer += Time.fixedDeltaTime;
        return true;
    }
    
    public Vector3 GetTargetToHit(Vector3 target, float heightOffset = 0.1f)
    {
        Vector3 diff = transform.position - target;
        diff.y = 0;
        return target + new Vector3(0, diff.magnitude * heightOffset, 0);
    }

#if ROBOOTCAMP
    public virtual void EnforceActivityRequirements() { }
#endif
}
