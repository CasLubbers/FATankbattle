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

        teamTanks = getTeamTanks();
    }

    public float Kc = 0;
    public float Ks = 0;
    public float Ka = 0;
    public float Kw = 200;
    public float WanderJitter = 50;
    public float WanderDistance = 50;
    public float WanderRadius = 3;
    public float bound = 70;
    public float maxDistanceCohesion = 169.420f;
    public float enemyVisability = 300f;

    public string tanksTag = "Tank";
    public string teamTanksTag = "Team1";

    static int teamPointIndex = 0;
    readonly float destMargin = 30.0f;
    GameObject[] teamTanks;

    Vector3 wanderTarget;
    GameObject debugWanderCube;

    bool wasFarAway;

    void cohesion(Transform player)
    {
        teamTanks = getTeamTanks();
        if (teamTanks.Length == 0) return;

        Vector3 center = GetCenter(teamTanks);

        if (Vector3.Distance(player.position, center) < maxDistanceCohesion) {
            if (wasFarAway) {
                player.GetComponent<NavMeshAgent>().destination = points[teamPointIndex].position;
                wasFarAway = false;
            }
            return;
        }
        
        center = GetCenter(getTeamTanks(player.gameObject));
        UpdateDestination((center + player.position) / 2, teamTanks);
        wasFarAway = true;
    }

    Vector3 GetCenter(GameObject[] tanks) {
        Vector3 total = new Vector3();
        foreach (var teamTank in tanks)
            total += teamTank.transform.position;
        return total / tanks.Length;
    }

    private GameObject[] getTeamTanks() {
        return GameObject.FindGameObjectsWithTag(teamTanksTag) ;
    }

    private GameObject[] getTeamTanks(GameObject exclude) {
        List<GameObject> teamTanks = new List<GameObject>(getTeamTanks());
        teamTanks.Remove(exclude);
        return teamTanks.ToArray();
    }

    Vector3 alignment()
    {
        Vector3 r = new Vector3();
        int countAgents = 0;

        var teamTanks = getTeamTanks();

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
            GotoNextPoint();
        }

        player.rotation = Quaternion.LookRotation(agent.velocity.normalized);
    }

    float RandomBinomial()
    {
        return UnityEngine.Random.Range(0f, 1f) - UnityEngine.Random.Range(0f, 1f);
    }

    public override void Reason(Transform enemy, Transform player)
    {
        List<GameObject> enemies = player.GetComponent<NPCTankController>().getEnemies();
        enemy = player.GetComponent<NPCTankController>().GetClosestEnemy(enemies);
        
        if (player != enemy)
        {
            //Check the distance with player tank
            //When the distance is near, transition to chase state
            if (Vector3.Distance(enemy.position, player.position) < enemyVisability)
            {
                Debug.Log("Switch to Chase State");
                player.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
                return;
            }
            Vector3 pos = player.position;
            pos.y += 5f;

            Ray collisionRay = new Ray(pos, player.forward);
            
            RaycastHit hit;

            if (Physics.Raycast(collisionRay, out hit, 100) && hit.transform.gameObject.tag == tanksTag) {
                Debug.Log("Switch to Evading State");
                player.GetComponent<NPCTankController>().SetTransition(Transition.Colliding);
            }
        }
    }

    public override void Act(Transform enemy, Transform player) {
        wander(player);
        cohesion(player);
    }

    void GotoNextPoint() {
        GameObject[] teamTanks = getTeamTanks();
        if (teamTanks.Length == 0) return;

        teamPointIndex = Random.Range(0, points.Length - 1);

        foreach (var teamTank in teamTanks)
            if (teamTank.GetComponent<NavMeshAgent>().enabled)
            {
                Debug.Log(points);
                Debug.Log(teamPointIndex);
                teamTank.GetComponent<NavMeshAgent>().destination = points[teamPointIndex].position;
            }
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