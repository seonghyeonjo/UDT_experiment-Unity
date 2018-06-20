using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
	Thread m_Thread;

	Encoding encode = Encoding.GetEncoding ("UTF-8");

	bool m_bRunning = false;

	bool m_bConnected = false;

	string strReceived = "";

	// Use this for initialization
	void Start()
	{
		ThreadStart ts = new ThreadStart(NetThread);

		m_Thread = new System.Threading.Thread(ts);
		m_Thread.Start ();

		m_bRunning = true;
	}

	void NetThread()
	{
		TcpListener tcpListener = null;
		try
		{
			tcpListener = new TcpListener(System.Net.IPAddress.Any, 9992);
			tcpListener.Start();
			while (m_bRunning)
			{
				// check if new connections are pending, if not, be nice and sleep 100ms
				if (!tcpListener.Pending())
				{
					Thread.Sleep(100);
					m_bConnected = false;
				}
				else
				{
					StreamReader Reader = null;
					Socket Client = null;
					NetworkStream Stream = null;

					Client = tcpListener.AcceptSocket();

					Stream = new NetworkStream(Client);
					Reader = new StreamReader(Stream, encode);

					while(Reader != null)
					{
						m_bConnected = true;
						if (!Reader.EndOfStream)
							strReceived = Reader.ReadLine();
						else
							break;
					}
                    if (Reader != null)
                        Reader.Close();
                    Stream.Close();
					Client.Close();
				}
			}
		}
		catch (ThreadAbortException)
		{
			tcpListener.Stop();
			m_bRunning = false;
			m_bConnected = false;
		}
		finally
		{
			tcpListener.Stop();
			m_bRunning = false;
			m_bConnected = false;
		}
	}

	public bool isConnected() {
		return m_bConnected;
	}

	public TCPData Parse_Data() {
		TCPData Data = new TCPData();
		if (strReceived.Length > 1) {
			string[] sub = strReceived.Split (' '); // data sended with ' '
            Data.message_number = int.Parse(sub[0]);
            if(Data.message_number == 7)
            {
                Data.m_fSpeed = float.Parse(sub[1]);
                Data.m_fComfSpd = float.Parse(sub[2]);
                Data.m_fRepeatTime = int.Parse(sub[3]);
                Data.m_fSteady_Time_Interval = float.Parse(sub[4]);
                Data.global_current_time = float.Parse(sub[5]);
                Data.body_detection = int.Parse(sub[6]);
            }
            else if(Data.message_number == 2)
            {
                Data.m_nMode = int.Parse(sub[1]);
            }
		} else {
            Debug.Log("Data is unparsable. Data : "+strReceived);
		}

		return Data;	// return data
	}

	public void stopListening() {
		m_bRunning = false;
		m_bConnected = false;
		// wait fpr listening thread to terminate (max. 500ms)
        m_Thread.Abort();
		m_Thread.Join(500);
	}

	void OnApplicationQuit() {
		// stop listening thread
		stopListening();
	}
}
