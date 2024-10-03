using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cinta : MonoBehaviour
{

    [SerializeField] private float Vel_cinta, Vel_textura;
    [SerializeField] private List<GameObject> OnBelt;
    [SerializeField] private GameObject locator;
    
    public bool Ensuelo  = false;
    public bool EnCentro = false;
    
    private Material material;
    private Vector3 dir_cinta;
    private Vector3 Velocidad;
    


    void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        material.mainTextureOffset -= new Vector2(1,0) * (Vel_textura/3) * Time.deltaTime;
        dir_cinta = locator.transform.position - transform.position;
    }


    void FixedUpdate() {
        Velocidad = Vel_cinta * dir_cinta; 
        for (int i = 0; i <= OnBelt.Count -1 ; i++)
        {
            if (EnCentro && Ensuelo)
            {    
                OnBelt[i].GetComponent<Rigidbody>().velocity = Velocidad;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        OnBelt.Add(collision.gameObject);
        Ensuelo = true;
    }

    private void OnCollisionExit(Collision collision) 
    {
        OnBelt.Remove(collision.gameObject);
        Ensuelo = false;
    }

    void OnTriggerStay(Collider collision) {
        EnCentro = true;
    }

    void OnTriggerExit(Collider collision) {
        EnCentro = false;
    }
}
