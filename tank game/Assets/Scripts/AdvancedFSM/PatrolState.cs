using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using UnityEngine.AI;

public class PatrolState : FSMState
{
    private Transform[] points;

    public PatrolState(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Patrolling;

        curRotSpeed = 2.0f;
        curSpeed = 100.0f;
    }

    public float Kc = 0;
    public float Ks = 0;
    public float Ka = 0;
    public float Kw = 200;
    public float WanderJitter = 50;
    public float WanderDistance = 50;
    public float WanderRadius = 3;
    public float bound = 70;
    public float distanceOtherTanks = 300;

    private static int teamPointIndex = 0;
    private readonly float destMargin = 30.0f;

    Vector3 wanderTarget;
    GameObject debugWanderCube;

    void cohesion(Transform player)
    {
        Vector3 r = new Vector3();
        bool nearbyOtherTank = false;

        var teamTanks = getTeamTanks("Team1");

        if (teamTanks.Length == 0) return;

        foreach (var agent in teamTanks) {
            if (agent.name != player.name) {
                r += agent.transform.position;
                Debug.Log("Aantal tanks: " + teamTanks.Length);
                Debug.Log("Distance: " + Vector3.Distance(player.position, agent.transform.position));
                if (Vector3.Distance(player.position, agent.transform.position) < distanceOtherTanks) {
                    nearbyOtherTank = true;
                    continue;
                }
            }
        }

        if (!nearbyOtherTank) {
            Debug.Log(player.name + " IS FAR AWAY");
            player.GetComponent<NPCTankController>().navAgent.SetDestination((r / (teamTanks.Length - 1)).normalized);
            //((r / teamTanks.Length) - player.transform.position).normalized;
        }
    }

    private GameObject[] getTeamTanks(String team) {
        GameObject[] teamTanks = GameObject.FindGameObjectsWithTag("Team1");

        for (int i = 0; i < teamTanks.Length; i++) {
            teamTanks[i] = teamTanks[i].transform.parent.gameObject;
        }
        return teamTanks;
    }

    Vector3 alignment()
    {
        Vector3 r = new Vector3();
        int countAgents = 0;

        var teamTanks = getTeamTanks("Team1");

        if (teamTanks.Length == 0) return r;

        foreach (var agent in teamTanks)
        {
            r += agent.transform.position;
            countAgents++;
        }

        r /= countAgents;

        return r.normalized;
    }

    void wrapAround(ref Vector3 v, float min, float max)
    {
        v.x = wrapAroundFloat(v.x, min, max);
        v.y = wrapAroundFloat(v.y, min, max);
        v.z = wrapAroundFloat(v.z, min, max);
    }

    float wrapAroundFloat(float value, float min, float max)
    {
        if (value > max) value = min;
        else if (value < min) value = max;
        return value;
    }

    protected void wander(Transform player)
    {
        NPCTankController tankController = player.GetComponent<NPCTankController>();
        NavMeshAgent agent = tankController.navAgent;

        if (points == null)
            points = tankController.points;

        if (Vector3.Distance(player.position, agent.destination) <= destMargin) {
            GotoNextPoint(agent);
        }
        agent.SetDestination(points[teamPointIndex].position);

        player.rotation = Quaternion.LookRotation(agent.velocity.normalized);
    }

    float RandomBinomial()
    {
        return UnityEngine.Random.Range(0f, 1f) - UnityEngine.Random.Range(0f, 1f);
    }

    public override void Reason(Transform enemy, Transform player)
    {
        if (player != enemy)
        {
            //Check the distance with player tank
            //When the distance is near, transition to chase state
            if (Vector3.Distance(enemy.position, player.position) <= 300.0f)
            {
                Debug.Log("Switch to Chase State");
                enemy.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
            }
            Vector3 pos = enemy.position;
            pos.y += 5f;

            Ray collisionRay = new Ray(pos, enemy.forward);

            // Debug.DrawRay(collisionRay.origin, collisionRay.direction * 200f, Color.blue);

            RaycastHit hit;
            if (Physics.Raycast(collisionRay, out hit, 200f))
            {
                Debug.Log("Switch to Evading State");
                enemy.GetComponent<NPCTankController>().SetTransition(Transition.Colliding);
            }
        }
    }

    public override void Act(Transform enemy, Transform player) {
        wander(player);
        cohesion(player);
    }

    void GotoNextPoint(NavMeshAgent agent) {

        if (points.Length == 0)
            return;
        agent.destination = points[teamPointIndex].position;
        teamPointIndex = Random.Range(0, points.Length);
    }
}