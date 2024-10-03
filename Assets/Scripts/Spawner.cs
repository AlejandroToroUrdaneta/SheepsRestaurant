using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject oveja;
    public GameObject ovejaPatrol;
    public static int contador;
    float intervalo = 5f;
    int maxSheeps = 32;
    [SerializeField]
    public static List<GameObject> sheeps = new List<GameObject>(); 



    void Start()
    {
        contador = 0;
        InvokeRepeating("Spawn", 0f, intervalo); //Llama a la funcion Spawn cada tantos segundos como ponga la variable intervalo
    }


    void Update()
    {

    }

    public void Spawn() //Crea Instancias del Prefab OvejaFeeder o OvejaPatrol
    {
        if (contador <= maxSheeps) //No pueden spawnear más ovejas que el maxSheep establecido
        {
            int rnd = Random.Range(0, 100);//Existe un 25% de probabilidad de spawnear una OvejaPatrol y un 75% de spawnear una OvejaFeeder
            if(rnd >= 40) 
            {
                GameObject gm = Instantiate(oveja, this.transform.position, this.transform.rotation, this.transform);
                gm.AddComponent<UnitFedeer>();//le asignamos su script al crearla
                sheeps.Add(gm);
                contador++;
            }
            else
            {
                GameObject gm = Instantiate(ovejaPatrol, this.transform.position, this.transform.rotation, this.transform);
                gm.AddComponent<UnitPatrol>();//le asignamos su script al crearla
                sheeps.Add(gm);
                contador++;
            }
        }
    }
}
