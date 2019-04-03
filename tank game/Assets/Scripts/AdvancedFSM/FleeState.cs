using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeState : FSMState
{
    public FleeState(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Flee;
        curRotSpeed = 2.0f;
        curSpeed = 100.0f;
    }

    public override void Reason(Transform enemy, Transform player)
    {
        //When the distance with player tank is far, transition to patrol state
        float dist = Vector3.Distance(player.position, enemy.position);
        if (dist >= 500.0f)
        {
            Debug.Log("Switch to Patrol State");
            player.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
        }
    }

    public override void Act(Transform enemy, Transform player)
    {
        Quaternion fleeRotation = Quaternion.LookRotation(enemy.position);
        var targetRotation = Quaternion.Inverse(fleeRotation);
        player.rotation = Quaternion.Slerp(player.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        Debug.Log(targetRotation.y);
        Debug.Log(player.rotation.y);
       
        player.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        // Debug.DrawRay(collisionRay.origin, collisionRay.direction * 200f, Color.blue);
        //Vector3 pos = player.position;
        //pos.y += 5f;

        //Ray collisionRay = new Ray(pos, player.forward);

        // Debug.DrawRay(collisionRay.origin, collisionRay.direction * 200f, Color.blue);

        //RaycastHit hit;
        //if (Physics.Raycast(collisionRay, out hit, 200f))
        //{
        //    Debug.Log("Switch to Evading State");
        //    player.GetComponent<NPCTankController>().SetTransition(Transition.Colliding);
        //}
    }
}
