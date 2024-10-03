using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.PlayerLoop.PreLateUpdate;

public class ObserverPatrol : MonoBehaviour
{
    public Transform player;
    public UnitPatrol unitPatrol;
    public GameObject unitMesh;
    public Material unitMaterial;
    public float alertRange = 9.5f;
    public int jumps = 2;


    float nextUpdate = 0f;
    float updateRate = 1f;

    float listeningRadius = 3f;

    public bool frenesiMode = false;
    public float frenesiTime = 0f;

    public bool m_IsPlayerInRange;

    private void Start()
    {
        player = GameObject.Find("JohnLemon").transform;
        unitPatrol = GetComponentInParent<UnitPatrol>();
        unitMaterial = unitMesh.GetComponent<SkinnedMeshRenderer>().material;
    }

    

    void OnTriggerEnter(Collider other)
    {
        if (other.transform == other.CompareTag("Target"))
        {

            if (!frenesiMode)
            {
                m_IsPlayerInRange = true;
                unitPatrol.scListener.radius = 0f;
                frenesiTime = 0f;
                frenesiMode = true;
                unitPatrol.questionMark.gameObject.SetActive(false);
                unitPatrol.exclmMark.gameObject.SetActive(true);
                unitPatrol.visionCone.gameObject.SetActive(false);
                unitMaterial.color = Color.red;
                AlertOthers(jumps);
            }
            else
            {
                unitPatrol.MoveTo(player.name);
                unitPatrol.visionCone.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.transform == other.CompareTag("Target"))
        {
            m_IsPlayerInRange = true;
            frenesiTime = 0f;
        }
    }

    void Update()
    {
        if (Time.time >= nextUpdate)
        {
            nextUpdate = Time.time + updateRate;

            if (frenesiMode && frenesiTime < 3f)
            {
                unitPatrol.MoveTo(player.name);
            }
            else if (frenesiTime >= 3f)
            {
                frenesiMode = false;
                unitPatrol.exclmMark.gameObject.SetActive(false);
                jumps = 2;
                unitMaterial.color = Color.white;
                unitPatrol.scListener.radius = listeningRadius;
                unitPatrol.MoveTo(unitPatrol.lastKnownObjective);
            }
        }
        if (frenesiMode)
        {
            frenesiTime += Time.deltaTime;
        }
    }

    public void AlertOthers(int jump)
    {
        foreach (GameObject g in Spawner.sheeps)
        {
            
            float distance = Vector3.Distance(this.transform.position, g.transform.position);
            if (distance < alertRange && jump > 0)
            {
                ObserverPatrol obp = null;
                ObserverFeeder obf = null;
                
                try
                {
                    obp = g.GetComponentInChildren<ObserverPatrol>();
                }catch (Exception e)
                {
                    obf = g.GetComponentInChildren<ObserverFeeder>();
                }
               


                if(obp != null)
                {
                    obp.m_IsPlayerInRange = true;
                    obp.unitPatrol.scListener.radius = 0f;
                    obp.frenesiTime = 0f;
                    obp.unitPatrol.questionMark.gameObject.SetActive(false);
                    obp.unitPatrol.exclmMark.gameObject.SetActive(true);
                    obp.unitMaterial.color = Color.red;
                    obp.jumps--;
                    obp.frenesiMode = true;
                }
                else if(obf != null)
                {
                    obf.m_IsPlayerInRange = true;   
                    obf.unitFedeer.scListener.radius = 0f;
                    obf.frenesiTime = 0f;
                    obf.unitFedeer.questionMark.gameObject.SetActive(false);
                    obf.unitFedeer.exclmMark.gameObject.SetActive(true);
                    obf.unitMaterial.color = Color.red;
                    obf.jumps--;
                    obf.frenesiMode = true;
                }
               
            }
        }
    }
}
