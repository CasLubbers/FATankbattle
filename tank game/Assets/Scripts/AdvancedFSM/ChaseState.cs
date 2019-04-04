using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class ChaseState : FSMState
{
    public ChaseState(Transform[] wp) 
    { 
        waypoints = wp;
        stateID = FSMStateID.Chasing;

        curRotSpeed = 2.0f;
        curSpeed = 100.0f;
    }

    public override void Reason(Transform enemy, Transform player)
    {
        //Set the target position as the player position
        destPos = player.position;

        //Check the distance with player tank
        //When the distance is near, transition to attack state
        float dist = Vector3.Distance(enemy.position, destPos);
        //Debug.Log("dist:" + dist);

        Debug.Log("dist chasestate:" + dist);
        if (dist <= 200.0f)
        {
            //player.GetComponent<NavMeshAgent>().destination = player.position;
            Debug.Log("Switch to Attack state");
            player.GetComponent<NPCTankController>().SetTransition(Transition.ReachPlayer);
            return;
        }
        //Go back to patrol is it become too far
        else if (dist >= 300.0f)
        {
            Debug.Log("Switch to Patrol state");
            player.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
            return;
        }
    }

    public override void Act(Transform enemy, Transform player)
    {

        UpdateDestination(enemy.transform.position, getTeamTanks());
        player.rotation = Quaternion.LookRotation(player.GetComponent<NavMeshAgent>().velocity.normalized);

        //Rotate to the target point
        //destPos = player.position;

        //Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
        //npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //Go Forward
        //npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }
    void UpdateDestination(Vector3 point, GameObject[] tanks)
    {
        foreach (var tank in tanks)
            tank.GetComponent<NavMeshAgent>().destination = point;
    }

    private GameObject[] getTeamTanks()
    {
        return GameObject.FindGameObjectsWithTag("Team1");
    }

}
