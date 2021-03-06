﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class SpeedTest_Passive2 : MonoBehaviour
{   
    Texture2D texTmp;

    bool m_bFirstRun = true;

    float m_fTreveled = 0.0f;
    float m_fElapsed = 0.0f;
    float m_fRate = 0.0f;
    float m_fTime_start = 0.0f;

    float m_fSpd = 0.0f;
    //float m_fDir = 0.0f;
    float m_fCSpd = 0.0f;
    TCPServer_PassiveActive m_TCPServer_PassiveActive = null;
    string m_strPath = null;    // string for datapath

    bool wink = false;

    public Scrollbar ArrowBar;
    public Scrollbar SpeedBar;

    public float minXValue;// = 0.076f; //min arrow speed position
    public float maxXValue;// = 1.0f;   //max arrow speed position
    private float minSpeed = 0.0f;      //real min arrow speed
    private float maxSpeed = 0.8f;      //real max arrow speed
    public float minXValue2;// = 0.011f; //min speed bar position
    public float maxXValue2;// = 1.0f;   //max speed bar position
    private float minSpeed2 = 0.0f; //real min speed
    private float maxSpeed2 = 0.8f; //real max speed


    float m_ftargetSpd = 0.0f;
    double m_ftime = 0.0f;
    float m_fFast = 0.0f;
    float m_fSlow = 0.0f;
    int m_fswitch = 0;
    int count_repeat = 0;

    public Text times;
    // Use this for initialization
    void Start()
    {
        // getting TCP server
        m_TCPServer_PassiveActive = this.gameObject.AddComponent<TCPServer_PassiveActive>();
    }
    // Update is called once per frame
    void Update()
    {
        if (m_TCPServer_PassiveActive.isConnected()) // if connected
            StartCoroutine(Contmove()); // coroutine Contmove()
        else if (m_fTime_start > 0)
        {   // disconnect after started
            // initializing
            //m_bFirstRun = true;
            // m_uIndex = 0;

            StopCoroutine(Contmove()); // stop coroutine - Contmove()

            m_TCPServer_PassiveActive.stopListening();   // stop TCP server

            // change scene to start scene
            SceneManager.UnloadScene("07_passive_mode");
            SceneManager.LoadScene("00_Start");
        }
    }

    System.Collections.IEnumerator Contmove()
    {
        if (m_TCPServer_PassiveActive.isConnected())
        {   // if connected
            if (m_bFirstRun)
            {
                m_fTime_start = Time.time;
                m_bFirstRun = false;
                m_strPath = Application.dataPath + "/" + DateTime.Now.ToString("yyyy-MM-dd_HHmm") + ".txt";
            }
            else
            {             // start-up ended
                // move forward by speed and delta time
                //Vector3 vMove = transform.TransformDirection(Vector3.forward);
                m_fSpd = m_TCPServer_PassiveActive.Parse_Data().m_fSpeed;
                m_fCSpd = m_TCPServer_PassiveActive.Parse_Data().m_fComfSpd;

                float fDt = Time.deltaTime;

                float fDtrevel = m_fSpd * fDt;

                //transform.Translate(vMove * m_fSpd * fDt);


                m_fTreveled += fDtrevel;    // treveled length
                m_fElapsed = Time.time - m_fTime_start; // elapsed time
                //m_fRate = 1 / fDt;          // frame rate
                using (System.IO.StreamWriter file =

                    new System.IO.StreamWriter(@m_strPath, true))
                {
                    file.WriteLine(m_fElapsed.ToString() + "\t" + m_fSpd.ToString() + "\n");
                }
                m_fFast = 1.25f * m_fCSpd;
                if (m_fFast >= 0.8)
                    m_fFast = 0.8f;
                m_fSlow = 0.75f * m_fCSpd;
                //target speed control


                if (m_fswitch == 0 && m_fSlow != 0)
                {
                    m_ftargetSpd = m_fSlow;
                    if (m_fSpd >= m_fSlow)
                        m_fswitch = 1;
                }

                if (m_fSpd <= m_fSlow && m_fswitch == 1) //to find when start to count in slow speed
                {
                    m_ftime = m_fElapsed;
                    m_fswitch = 2; 
                    count_repeat++;
                }

                if (m_ftargetSpd >= m_fSlow && (m_fElapsed - m_ftime) >= 10 && m_fswitch == 2)
                    m_ftargetSpd = m_fFast;            

                if (m_fSpd >= m_fFast && m_fswitch == 2)
                {
                    m_ftime = m_fElapsed;
                    m_fswitch = 1;
                }

                if (m_ftargetSpd >= m_fFast && (m_fElapsed - m_ftime) >= 10 && m_fswitch == 1)
                {
                    if (count_repeat == 3) //when repeat 3 time, stop
                        m_ftargetSpd = 0;                   
                    else
                        m_ftargetSpd = m_fSlow;                    
                }

                ArrowBar.value = m_ftargetSpd * (maxXValue - minXValue) / (maxSpeed - minSpeed) + minXValue;
                SpeedBar.size = m_fSpd * (maxXValue2 - minXValue2) / (maxSpeed2 - minSpeed2) + minXValue2;

                times.text = count_repeat.ToString();
            }
        }
        else
        {   // if disconnected, display 'start.jpg'
            string url = "file://" + Application.dataPath + "/../frames/start.jpg";

            var www = new WWW(url);
            yield return www;

            texTmp = new Texture2D(1920, 1080, TextureFormat.DXT1, false);
            www.LoadImageIntoTexture(texTmp);
        }
        wink = !wink;
    }

    void OnGUI()
    {// GUI Messaging
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texTmp, ScaleMode.ScaleToFit);
        string disp;
        Rect textbox;
        if (m_TCPServer_PassiveActive.isConnected() )
        {   // connected
            // lower bound of display
            textbox = new Rect(Screen.width / 2 - 100, Screen.height - 100, 200, 100);

            disp = "Vel : " + m_fSpd.ToString()                         // speed
                //+ "\nFrameRate : " + Math.Floor(m_fRate + 0.5).ToString()   // framerate
                + "\nElapsed : " + m_fElapsed.ToString()                    // elapsed time
                + "\nTreveled : " + m_fTreveled.ToString();             // treveled length
        }
        else
        {//disconnected
            // center of display
            textbox = new Rect(Screen.width / 2 - 100, Screen.height + 150, 200, 100);
            /*
            // winking "Now waiting..."
            if ((Time.time * 4) % 2 < 1)
                disp = "여기를 보세요";
            else
                disp = "";
            disp += "\n" + Math.Floor((m_uFin - m_uIndex) / 60.0f).ToString();*/
            disp = "여기를 보세요";

        }
        GUI.TextArea(textbox, disp);
    }
}