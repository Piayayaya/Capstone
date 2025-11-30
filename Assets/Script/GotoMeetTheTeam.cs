using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoMeetTheTeam : MonoBehaviour
{

    [SerializeField] private string MeetTheTeamSceneName = "MeetTheTeam";
    // use EXACT scene name

    public void OpenMeetTheTeam()
    {
        SceneManager.LoadScene(MeetTheTeamSceneName);
    }
}
