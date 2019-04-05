using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

public class T1ChaseState : T1FSMState
{
    public float attackDistance = 180f;
    public float lostDistance = 300f;
    public string teamTanksTag = "Team1";

    public T1ChaseState(Transform[] wp) 
    { 
        waypoints = wp;
        stateID = FSMStateID.Chasing;

        curRotSpeed = 2.0f;
        curSpeed = 100.0f;
    }

    public override void Reason(Transform enemy, Transform player)
    {
        List<GameObject> enemies = player.GetComponent<T1NPCTankController>().getEnemies();
        enemy = player.GetComponent<T1NPCTankController>().GetClosestEnemy(enemies);

        if (enemy != player)
        {

            //Check the distance with player tank
            //When the distance is near, transition to attack state
            float enemyDistance = Vector3.Distance(enemy.position, player.position);

            if (enemyDistance <= attackDistance)
            {
                Debug.Log("Switch to Attack state");
                player.GetComponent<T1NPCTankController>().SetTransition(Transition.ReachPlayer);
                if (enemy != null)
                    T1AttackState.target = enemy;
                return;
            }
            //Go back to patrol is it become too far
            else if (enemyDistance >= lostDistance)
            {
                Debug.Log("Switch to Patrol state");
                player.GetComponent<T1NPCTankController>().SetTransition(Transition.LostPlayer);
                return;
            }
        }
    }

    public override void Act(Transform enemy, Transform player)
    {
        List<GameObject> enemies = player.GetComponent<T1NPCTankController>().getEnemies();
        enemy = player.GetComponent<T1NPCTankController>().GetClosestEnemy(enemies);
        if (player != enemy)
        {
            UpdateDestination(enemy.transform.position, GameObject.FindGameObjectsWithTag(teamTanksTag));
        }
        player.rotation = Quaternion.LookRotation(player.GetComponent<NavMeshAgent>().velocity.normalized);
        
    }

    void UpdateDestination(Vector3 point, GameObject[] tanks)
    {
        foreach (var tank in tanks)
            if (tank.GetComponent<NavMeshAgent>().enabled)
            {
                tank.GetComponent<NavMeshAgent>().destination = point;
            }
    }
}
