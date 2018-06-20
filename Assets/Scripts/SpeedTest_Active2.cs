using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class SpeedTest_Active2 : MonoBehaviour
{
    Texture2D texTmp;

    bool m_bFirstRun = true;

    float m_fTreveled = 0.0f;
    float m_fElapsed = 0.0f;
    float m_fRate = 0.0f;
    float m_fTime_start = 0.0f;

    float m_fSpd = 0.0f;
    float m_fCSpd = 0.0f;
    TCPServer_PassiveActive m_TCPServer_PassiveActive = null;
    string m_strPath = null;    // string for datapath

    //bool wink = false;

    public Scrollbar ArrowBar;
    public Scrollbar SpeedBar;


    public float minXValue;// = 0.075f; //min arrow position
    public float maxXValue;// = 1.0f;   //max arrow position
    private float minSpeed = 0.0f;      //real min target speed
    private float maxSpeed = 0.8f;      //real max target speed
    public float minXValue2;// = 0.011f; //min speed bar position
    public float maxXValue2;// = 1.0f;   //max speed bar position
    private float minSpeed2 = 0.0f; //real min speed
    private float maxSpeed2 = 0.8f; //real max speed

    //int m_switch = 0;
    float m_ftargetSpd = 0.0f; //current arrow speed position
    double m_ftime = 0.0f; //starting time counting 20sec
    float m_fFast = 0.0f; // fast speed
    float m_fSlow = 0.0f; // slow speed
    int m_fswitch = 0; //
    float m_fchangetime = 0;
    int m_count = 0; // counting 60frame(2second)
    float count = 0; // counting second maintaining 2 sec
    int count_repeat = 0; //counting how many repeat the sequence

    public Text counting;
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
        else if (m_fTime_start >0)
        {   // disconnect after started
            // initializing
            m_bFirstRun = true;

            StopCoroutine(Contmove()); // stop coroutine - Contmove()

            m_TCPServer_PassiveActive.stopListening();   // stop TCP server

            // change scene to start scene
            SceneManager.UnloadScene("08_active_mode");
            SceneManager.LoadScene("00_Start");
        }
    }

    System.Collections.IEnumerator Contmove()
    {
        if (m_TCPServer_PassiveActive.isConnected())
        {   // if connected
            if (m_bFirstRun)
            {
                m_bFirstRun = false;        //start flag to false

                // setting datapath
                m_strPath = Application.dataPath + "/" + DateTime.Now.ToString("yyyy-MM-dd_HHmm") + ".txt";

                // setting start time
                m_fTime_start = Time.time;
            }
            else
            {   // start-up ended
                // move forward by speed and delta time
                //Vector3 vMove = transform.TransformDirection(Vector3.forward);
                m_fSpd = m_TCPServer_PassiveActive.Parse_Data().m_fSpeed;
                m_fCSpd = m_TCPServer_PassiveActive.Parse_Data().m_fComfSpd;

                float fDt = Time.deltaTime;

                float fDtrevel = m_fSpd * fDt;

                //transform.Translate(vMove * m_fSpd * fDt);

                m_fTreveled += fDtrevel;    // treveled length
                m_fElapsed = Time.time - m_fTime_start; // elapsed time

                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@m_strPath, true))
                {
                    file.WriteLine(m_fElapsed.ToString() + "\t" + m_fSpd.ToString() + "\n");
                }
                m_fFast = 1.25f * m_fCSpd;
                m_fSlow = 0.75f * m_fCSpd;
                //target speed control

                if (m_fswitch == 0 && m_fSlow != 0)
                {
                    m_ftargetSpd = m_fSlow;
                    if (m_fSpd >= m_fSlow-0.13f)
                        m_fswitch = 1;
                }

                if (m_fswitch == 1 && m_ftargetSpd == m_fSlow)
                {
                    // 0.13/0.8*(1-0.011)=0.161
                    // 0.13 : half width of speed, 0.8 : maximum speed, 1 : whole width of frame,  0.11 : minimum value of frame, 0.161 : half width of blue bar
                    if (m_fSpd <= m_fSlow +0.13f && m_fSpd >= m_fSlow -0.13f)
                        m_count = m_count + 1;
                    else
                        m_count = 0;

                    if (m_count >= 60)
                    {
                        m_fswitch = 2;
                        m_ftime = m_fElapsed;
                        count_repeat++;
                    }
                }

                if (m_fswitch == 2 && m_ftargetSpd == m_fFast)
                {
                    if (m_fSpd <= m_fFast + 0.1335f && m_fSpd >= m_fFast -0.1335f)
                        m_count = m_count + 1;
                    else
                        m_count = 60;

                    if (m_count >= 120)
                    {
                        m_fswitch = 1;
                        m_ftime = m_fElapsed;
                        m_count = 120;

                    }
                }

                if (m_ftargetSpd == m_fSlow && (m_fElapsed - m_ftime) >= 10 && m_fswitch == 2)
                    m_ftargetSpd = m_fFast;

                if (m_ftargetSpd == m_fFast && (m_fElapsed - m_ftime) > 10 && m_fswitch == 1)
                {
                    if (count_repeat == 3)
                        m_ftargetSpd = 0;
                    else
                        m_ftargetSpd = m_fSlow;
                }

                ArrowBar.value = m_ftargetSpd * (maxXValue - minXValue) / (maxSpeed - minSpeed) + minXValue;
                SpeedBar.size = m_fSpd * (maxXValue2 - minXValue2) / (maxSpeed2 - minSpeed2) + minXValue2;

                count = m_count / 6;

                counting.text = count.ToString();
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
        //wink = !wink;
    }

    void OnGUI()
    {    // GUI Messaging
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texTmp, ScaleMode.ScaleToFit);
        string disp;
        Rect textbox;
        if (m_TCPServer_PassiveActive.isConnected())
        {   // connected
            // lower bound of display
            textbox = new Rect(Screen.width / 2 - 100, Screen.height - 100, 200, 100);

            disp = "Vel : " + m_fSpd.ToString()                         // speed
                //+ "\nFrameRate : " + Math.Floor(m_fRate + 0.5).ToString()   // framerate
                //+ "\nCount : " + count.ToString()                    // count number
                + "\nElapsed : " + m_fElapsed.ToString()                    // elapsed time
                + "\nTreveled : " + m_fTreveled.ToString();             // treveled length
        }
        else
        {//disconnected
            // center of display
            textbox = new Rect(Screen.width / 2 - 100, Screen.height / 2 - 50, 200, 100);
            /*
             // winking "Now waiting..."
            if ((Time.time * 4) % 2 < 1)
                disp = "여기를 보세요";
            else
                disp = "";
            disp += "\n" + Math.Floor((m_uFin - m_uIndex) / 60.0f).ToString();*/
            disp = "";
        }
        GUI.TextArea(textbox, disp);
    }
}