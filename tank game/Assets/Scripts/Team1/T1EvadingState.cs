using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class T1EvadingState : T1FSMState
{
    public T1EvadingState(Transform[] wp)
    {
        this.waypoints = wp;
        stateID = FSMStateID.Evading;

        curRotSpeed = 2.0f;
        curSpeed = 20.0f;
    }

    public override void Act(Transform enemy, Transform player)
    {
        // Evade the tank
        player.GetComponent<NavMeshAgent>().isStopped = true;
    }

    public override void Reason(Transform enemy, Transform player)
    {
        Vector3 pos = player.position;
        pos.y = pos.y += 5f;

        Ray forwardRay = new Ray(pos, player.forward);
        RaycastHit hit;

        if (!Physics.Raycast(forwardRay, out hit, 100) || (Physics.Raycast(forwardRay, out hit, 100) && hit.transform.gameObject.tag != "Untagged")) {
            player.GetComponent<NavMeshAgent>().isStopped = false;
            Debug.Log("Switch to Patrol state");
            player.GetComponent<T1NPCTankController>().SetTransition(Transition.LostPlayer);
            return;
        }
    }
}
