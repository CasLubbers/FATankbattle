using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeState : FSMState
{
    public FleeState(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Flee;
        curRotSpeed = 1.0f;
        curSpeed = 100.0f;
    }

    public override void Reason(Transform player, Transform npc)
    {
        //When the distance with player tank is far, transition to patrol state
        float dist = Vector3.Distance(npc.position, player.position);
        if (dist >= 500.0f)
        {
            Debug.Log("Switch to Patrol State");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
        }
    }

    public override void Act(Transform player, Transform npc)
    {
        Quaternion fleeRotation = Quaternion.LookRotation(player.position);
        var targetRotation = Quaternion.Inverse(fleeRotation);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        Debug.Log(targetRotation.y);
        Debug.Log(npc.rotation.y);
       
        npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        // Debug.DrawRay(collisionRay.origin, collisionRay.direction * 200f, Color.blue);
        Vector3 pos = npc.position;
        pos.y += 5f;

        Ray collisionRay = new Ray(pos, npc.forward);

        // Debug.DrawRay(collisionRay.origin, collisionRay.direction * 200f, Color.blue);

        RaycastHit hit;
        if (Physics.Raycast(collisionRay, out hit, 200f))
        {
            Debug.Log("Switch to Evading State");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.Colliding);
        }
    }
}
