using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI talkText;
    public TalkManager talkManager;
    public GameObject talkPanel;
    public GameObject scanObject;
    public GameObject PlayerUI;

    bool isAction;
    public int talkindex;

    Player player;
    public Player MainPlayer => player;

    static GameManager instance = null;
    public static GameManager Inst
    {
        get => instance;
    }

    public void Action(GameObject scanObj)
    {
        if (isAction)
        {
            isAction = false;            
        }
        else
        {
            isAction = true;
            scanObject = scanObj;
            ObjectData objData = scanObject.GetComponent<ObjectData>();
            Talk(objData.id, objData.isNPC);
            Debug.Log("상호작용");
        }
        talkPanel.SetActive(isAction);
    }

    void Talk(int id, bool isNpc)
    {
        string talkData = talkManager.GetTalk(id, talkindex);
        if (isNpc) //대상이 npc일때
        {
            talkText.text = talkData;
            //PlayerUI.SetActive(false);
        }
        else // 상자같은거 일때
        {
            talkText.text = talkData;
            //PlayerUI.SetActive(true);
        }
    }

    private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Initialize();
    }

    private void Initialize()
    {
        player = FindObjectOfType<Player>();
    }
}
