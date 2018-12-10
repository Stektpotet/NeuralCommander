using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(UnitActions), typeof(UnitSensor), typeof(UnitComplexActions))]
[RequireComponent(typeof(CoverActivity), typeof(PatrolActivity), typeof(ThrowAtPlayerActivity))]
[RequireComponent(typeof(Rigidbody), typeof(SuppressActivity))]
public class SquadUnit : MonoBehaviour
{
    public const int SENSOR_COUNT = 11;
    public int squadUnitIndex;

    [SerializeField]
    private UnitActions _actions;

    [SerializeField]
    private UnitComplexActions _complexActions;
    public UnitComplexActions ComplexActions => _complexActions;
    public UnitActions Actions => _actions;

    [SerializeField]
    private UnitSensor _sensor;
    public UnitSensor Sensor => _sensor;

    [SerializeField]
    private Animator _stateMachineController;
    public Animator StateMachineController => _stateMachineController;
    
    bool storedIsInCover;
    int observationsSinceIsInCover;

    [SerializeField] float destructionForce = 10;

    private void OnDestroy()
    {
        var rightHandItem = Sensor.rightHand.GetComponentInChildren<PhysicalObject>();
        if(rightHandItem != null)
        {
            Actions.DropObject(
                rightHandItem, Sensor.rightHand, 
                Sensor.GetVelocity() + new Vector3(
                    (transform.right.x + Random.value) * destructionForce, 
                    (transform.right.y +  Random.value) * destructionForce,
                    (transform.right.z + Random.value) * destructionForce
                ) 
            ); //pls no nullref
            Destroy(rightHandItem.gameObject, 10);
        }
        var leftHandItem = Sensor.leftHand.GetComponentInChildren<PhysicalObject>();
        if (leftHandItem != null)
        {
            Actions.DropObject(
                leftHandItem, Sensor.leftHand,
                Sensor.GetVelocity() + new Vector3(
                    (-transform.right.x + Random.value) * destructionForce,
                    (-transform.right.y + Random.value) * destructionForce,
                    (-transform.right.z + Random.value) * destructionForce
                )
            ); //pls no nullref
            Destroy(leftHandItem.gameObject, 10);
        }
        //foreach(var physObj in GetComponentsInChildren<PhysicalObject>())
        //{
        //      Detatch from parent and add
        //      expotion force on all remaining phys-components
        //      kill after a certain amount of time if not picked up
        //  TODO: Add timer destruction in pHysicalObject
        //}
    }

    //Observations
    public bool[] GetObservations()
    {

        bool[] observations = new bool[SENSOR_COUNT]; 
        observationsSinceIsInCover += 1;

        observations[0] = false; //NOT DEAD
        observations[1] = _sensor.IsTakingDmg;
        if(observations[1] || observationsSinceIsInCover > 5)
        {
            observations[2] = storedIsInCover = _sensor.IsInCover;
            observationsSinceIsInCover = 0;
        }
        else
            observations[2] = storedIsInCover;
        
        observations[3] = _sensor.IsGrounded;
        observations[4] = _sensor.IsCrouching;
        observations[5] = _sensor.IsWounded;
        observations[6] = _sensor.IsArmed;
        observations[7] = _sensor.IsLoaded;
        observations[8] = _sensor.IsSeeingPlayer;
        observations[9] = _sensor.KnowsPlayerPosition;
        observations[10] = _sensor.IsObjectiveCompleted;
        return observations;
    }

    public float[] GetFloatObservations()
    {
        return new float[]
        {
            _sensor.Health,
        };
    }
    private void Start()
    {
        //_sensor.selfObject.onHit.AddListener(c=> )
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 21) // 21 = agentlayer
        {
            var otherAgent = other.GetComponent<SquadUnit>();

            if (otherAgent != null)
            {
                if (otherAgent.squadUnitIndex > squadUnitIndex)
                {
                    Vector3 relativeVector = (transform.position - other.transform.position).normalized;
                    _actions.BreakSpeed(1f);
                    _actions.MoveTowards(transform.position + relativeVector, _complexActions.moveSpeed * 10f);
                }
            }
        }
    }
}
