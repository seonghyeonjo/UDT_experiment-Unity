using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;


public class OnStart : MonoBehaviour {
	uint mode = 0;
	TCPServer m_TCPServer = null;
	// Use this for initialization
	void Start ()
    {
        DateTime time = DateTime.Now;
        Debug.Log("Update @ "+time.Hour+"@"+ time.Minute+"@"+time.Second+ "@" + time.Millisecond);
        m_TCPServer = this.gameObject.AddComponent <TCPServer>();
	}

	// Update is called once per frame
	void Update () {
        mode = (uint)m_TCPServer.Parse_Data ().m_nMode;
		switch (mode) {
		case 7:
			m_TCPServer.stopListening ();
			SceneManager.UnloadScene ("00_Start");
            SceneManager.LoadScene("01_passive_mode");
                break;
		//case 4:
		//	m_TCPServer.stopListening ();
		//	SceneManager.UnloadScene ("00_Start");
  //          SceneManager.LoadScene("02_passive_mode2");
  //          	break;
		case 8:
			m_TCPServer.stopListening ();
			SceneManager.UnloadScene ("00_Start");
            SceneManager.LoadScene ("03_active_mode");
                break;           
        //case 6:
        //    m_TCPServer.stopListening ();
        //    SceneManager.UnloadScene ("00_Start");
        //    SceneManager.LoadScene ("04_active_mode2");
        //        break;
		default:
			break;
		}
	}

	void OnGUI () {
		GUILayout.BeginArea (new Rect (Screen.width / 2 + 400, Screen.height / 2 - 250, 100, 430));

        if (GUILayout.Button("Passive mode", GUILayout.Height(50), GUILayout.Width(100)))
        {
            m_TCPServer.stopListening();
            SceneManager.UnloadScene("00_Start");
            SceneManager.LoadScene("01_passive_mode");
        }
        GUILayout.Space(10);
        if (GUILayout.Button ("Active mode", GUILayout.Height (50), GUILayout.Width (100))) {
			m_TCPServer.stopListening ();
			SceneManager.UnloadScene ("00_Start");
            SceneManager.LoadScene("03_active_mode");
        }

        GUILayout.Space(10);

		if (GUILayout.Button ("종료하기", GUILayout.Height (50), GUILayout.Width (100))) {
			Application.Quit ();
		}
        
        GUILayout.EndArea();
        /*
		string disp = null;

		if ((Time.time * 4) % 2 < 1)
			disp = "Now waiting..";
		else
			disp = "";

		disp += "\n" + mode.ToString ();
		GUI.TextArea (new Rect(Screen.width/2 - 100,Screen.height/2 - 50,200,100),disp);*/
	}
}
