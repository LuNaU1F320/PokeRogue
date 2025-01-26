using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] List<Text> SelectionTexts;
    int currentSelection = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleActionSelection();
    }
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection++;
            if (currentSelection > 4)
            {
                currentSelection = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection--;
            if (currentSelection < 0)
            {
                currentSelection = 4;
            }
        }
        UpdateActionSelection(currentSelection);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (currentSelection == 0)  //Continue
            {
                Debug.Log("Continue");
            }
            if (currentSelection == 1)  //NewGame
            {
                // Debug.Log("NewGame");
                SceneManager.LoadScene("PartyScene");
            }
            if (currentSelection == 2)  //LoadGame
            {
                Debug.Log("LoadGame");
            }
            if (currentSelection == 3)  //DailyRun
            {
                Debug.Log("DailyRun");
            }
            if (currentSelection == 4)  //Settings
            {
                Debug.Log("Settings");
            }
        }
    }
    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < SelectionTexts.Count; i++)
        {
            if (i == selectedAction)
            {
                SelectionTexts[i].color = Color.cyan;
            }
            else
            {
                SelectionTexts[i].color = Color.white;
            }
        }
    }
}
