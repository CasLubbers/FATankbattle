using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EvadingState : FSMState
{
    public EvadingState(Transform[] wp)
    {
        this.waypoints = wp;
        stateID = FSMStateID.Evading;

        curRotSpeed = 2.0f;
        curSpeed = 20.0f;
    }

    public override void Act(Transform enemy, Transform player)
    {
        player.GetComponent<NavMeshAgent>().isStopped = true;
        //Quaternion collisionRotation = Quaternion.LookRotation(destPos - npc.position);
        //Quaternion targetRotation = collisionRotation;
        //targetRotation.y = targetRotation.y + 2;
        //npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        ////Go Forward
        //npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    public override void Reason(Transform enemy, Transform player)
    {
        Vector3 pos = player.position;
        pos.y = pos.y += 5f;

        Ray forwardRay = new Ray(pos, player.forward);
        //Debug.DrawRay(pos, player.forward * 100, Color.red);
        RaycastHit hit;

        //Debug.DrawRay(forwardRay.origin, forwardRay.direction * 700f, Color.green);

        if (!Physics.Raycast(forwardRay, out hit, 100) || (Physics.Raycast(forwardRay, out hit, 100) && hit.transform.gameObject.tag != "Tank")) {
            player.GetComponent<NavMeshAgent>().isStopped = false;
            Debug.Log("Switch to Patrol state");
            player.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
        }
    }
}
