using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PatrolState : FSMState
{
    public PatrolState(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Patrolling;

        curRotSpeed = 1.0f;
        curSpeed = 100.0f;
    }
    
    public float Kc = 45;
    public float Ks = 90;
    public float Ka = 90;
    public float Kw = 20;
    public float WanderJitter = 50;
    public float WanderDistance = 50;
    public float WanderRadius = 3;
    public float bound = 70;

    Vector3 wanderTarget;
    GameObject debugWanderCube;

    Vector3 cohesion(Transform player)
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
        if (countAgents == 0) return r;

        r /= countAgents;

        // r /= neighs.Count;

        r = r - player.transform.position;

        return r.normalized;
    }

    Vector3 separation(Transform player)
    {
        Vector3 r = new Vector3();
        int countAgents = 0;

        GameObject[] teamTanks = getTeamTanks("Team1");

        if (teamTanks.Length == 0)
            return r;

        foreach (var agent in teamTanks)
        {
            countAgents++;
            Vector3 towardsMe = player.transform.position - agent.transform.position;
            if (towardsMe.magnitude > 0)
            {
                r += towardsMe.normalized / towardsMe.magnitude;
            }
        }

        r /= countAgents;

        return r.normalized;
    }

    private GameObject[] getTeamTanks(String team)
    {
        GameObject[] teamTanks = GameObject.FindGameObjectsWithTag("Team1");

        for (int i = 0; i < teamTanks.Length; i++)
        {
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

    virtual protected Vector3 combine(Transform player)
    {
        return Kc * cohesion(player) + Ks * separation(player) + Ka * alignment() + Kw * wander(player);
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

    protected Vector3 wander(Transform player)
    {
        float jitter = WanderJitter * Time.deltaTime;

        wanderTarget = new Vector3(RandomBinomial() * jitter, 0, RandomBinomial() * jitter); // +=

        wanderTarget = wanderTarget.normalized;

        wanderTarget *= WanderRadius;

        Vector3 targetInLocalSpace = wanderTarget + new Vector3(0, 0, WanderDistance);

        Vector3 targetInWorldSpace = player.transform.TransformPoint(targetInLocalSpace);

        targetInWorldSpace -= player.transform.position;
        return targetInWorldSpace.normalized;
    }

    float RandomBinomial()
    {
        return UnityEngine.Random.Range(0f, 1f) - UnityEngine.Random.Range(0f, 1f);
    }

    public override void Reason(Transform player, Transform npc)
    {
        //Check the distance with player tank
        //When the distance is near, transition to chase state
        if (Vector3.Distance(npc.position, player.position) <= 300.0f)
        {
            Debug.Log("Switch to Chase State");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
        }

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

    public override void Act(Transform player, Transform npc)
    {
        float t = Time.deltaTime;

        Vector3 a = combine(player);

        Vector3 v = a * t;

        Vector3 x = player.position + v * t;

        wrapAround(ref x, -bound, bound);

        player.transform.position = x;

        if (v.magnitude > 0)
            player.transform.LookAt(x + v);
    }
}