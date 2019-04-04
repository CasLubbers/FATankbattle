using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FleeState : FSMState
{
    private bool firstTime = true;

    public FleeState(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Flee;
        curRotSpeed = 2.0f;
        curSpeed = 100.0f;
    }

    public override void Reason(Transform enemy, Transform player)
    {
        if (player.GetComponent<NavMeshAgent>().remainingDistance <= 5f)
        {
            Debug.Log("Switch to Chase State");
            player.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
            return;
        }
    }

    public override void Act(Transform enemy, Transform player)
    {
        if (firstTime)
        {
            GameObject[] teamTanks = GameObject.FindGameObjectsWithTag("Team1");
            Transform furthestTeamTank = teamTanks[0].transform;
            for (int i = 1; i < teamTanks.Length; i++)
            {
                if (Vector3.Distance(teamTanks[i].transform.position, player.position) > Vector3.Distance(furthestTeamTank.position, player.position))
                    furthestTeamTank = teamTanks[i].transform;
            }
            player.GetComponent<NavMeshAgent>().SetDestination(furthestTeamTank.transform.position);
            firstTime = false;
        }

        player.rotation = Quaternion.LookRotation(player.GetComponent<NavMeshAgent>().velocity.normalized);
    }
}
