using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class SquadAcademy : Academy {
    public override void InitializeAcademy()
    {
        UnityEngine.AI.NavMesh.RemoveAllNavMeshData();
    }
}
