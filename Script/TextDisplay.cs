using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextDisplay : MonoBehaviour
{

    public Text textDisplay;
    public GameObject btn;
    public string[] conversations;
    int i;

    void Start()
    {
        StartCoroutine(TypeSentence());
    }

    void Update()
    {
        if (textDisplay.text == conversations[i])
        {
            btn.SetActive(true);
            if(Input.GetKeyDown(KeyCode.Z))
            {
                gotoNextSentence();
            }
        }
        else
        {
            btn.SetActive(false);
        }
    }

    IEnumerator TypeSentence()
    {
        foreach (char l in conversations[i].ToCharArray())
        {
            textDisplay.text += l;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void gotoNextSentence()
    {
        //ç≈å„ÇÃï™Ç‹Ç≈ìûíBÇµÇƒÇ¢Ç»Ç¢èÍçá
        if (i < conversations.Length - 1)
        {
            i++;
            textDisplay.text = "";
            StartCoroutine(TypeSentence());
        }
        else
        {
            Talk.instance.talkOut();
        }
    }
}
