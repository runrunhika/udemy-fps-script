using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Talk : MonoBehaviour
{
    public static Talk instance;

    [SerializeField] GameObject talkPanel;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            talkPanel.SetActive(true);
        }
    }

    public void talkOut()
    {
        talkPanel.SetActive(false);
    }
}
