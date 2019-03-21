using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadingState : FSMState
{
    public EvadingState(Transform[] wp)
    {
        this.waypoints = wp;
        stateID = FSMStateID.Evading;

        curRotSpeed = 1.0f;
        curSpeed = 20.0f;
    }

    public override void Act(Transform player, Transform npc)
    {
        Quaternion collisionRotation = Quaternion.LookRotation(destPos - npc.position);
        Quaternion targetRotation = collisionRotation;
        targetRotation.y = targetRotation.y + 2;
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //Go Forward
        npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    public override void Reason(Transform player, Transform npc)
    {
        Vector3 pos = npc.position;
        pos.y = pos.y += 5f;

        Ray forwardRay = new Ray(pos, npc.forward);
        RaycastHit hit;

        //Debug.DrawRay(forwardRay.origin, forwardRay.direction * 700f, Color.green);

        if (!Physics.Raycast(forwardRay, out hit, 700f))
        {
            FindNextPoint();
            Debug.Log("Switch to Patrol state");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
        }
    }
}
