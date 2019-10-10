using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using UnityEngine.UI;
using System;

public class Walk : MonoBehaviour
{

	//socket 和缓冲区
	Socket socket;
	const int BUFFER_SIZE = 1024;
	public byte[] readBuff = new byte[BUFFER_SIZE];

	//玩家列表
	Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();

	//消息列表
	List<string> msgList = new List<string>();
	//Player 预设
	public GameObject prefab;
	//自己的id和端口
	string id;


    // Start is called before the first frame update
    void Start()
    {
		Connect();
		UnityEngine.Random.seed = (int)DateTime.Now.Ticks;//随机位置
		float x = 100 + UnityEngine.Random.Range(-30, 30);
		float y = 0;
		float z = 100 + UnityEngine.Random.Range(-30, 30);
		Vector3 pos = new Vector3(x, y, z);
		AddPlayer(id, pos);
		SendPos();
	}

    // Update is called once per frame
    void Update()
    {
        for(int i =0;i<msgList.Count;i++)
		{
			HandleMsg();
		}
		Move();//移动
    }

	void AddPlayer(string id,Vector3 pos)
	{
		GameObject player = (GameObject)Instantiate(prefab, pos, Quaternion.identity);
		TextMesh textMesh = player.GetComponentInChildren<TextMesh>();
		textMesh.text = id;
		players.Add(id, player);
	}

	void SendPos()//发送位置协议
	{
		GameObject player = players[id];
		Vector3 pos = player.transform.position;
		//组装协议

		string str = "POS ";
		str += id + " ";
		str += pos.x.ToString() + " ";
		str += pos.y.ToString() + " ";
		str += pos.z.ToString() + " ";

		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);

		socket.Send(bytes);
		Debug.Log("发送" + str);
	}

	void SendLeave()//发送离开协议
	{
		string str = "LEAVE";
		str += id + " ";
		byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
		socket.Send(bytes);
		Debug.Log("发送" + str);
	}

	 void Move()//移动
	{
		if (id == " ")
			return;
		GameObject player = players[id];
		if(Input.GetKey(KeyCode.UpArrow))//上
		{
			player.transform.position += new Vector3(0, 0, 1);
			SendPos();
		}
		else if(Input.GetKey(KeyCode.DownArrow))//下
		{
			player.transform.position += new Vector3(0, 0, -1);
			SendPos();
		}
		else if (Input.GetKey(KeyCode.LeftArrow))//左
		{
			player.transform.position += new Vector3(-1, 0, 0);
			SendPos();
		}
		else if (Input.GetKey(KeyCode.RightArrow))//右
		{
			player.transform.position += new Vector3(1, 0, 0);
			SendPos();
		}
	}

	private void OnDestroy()
	{
		SendLeave();
	}

	private void Connect()
	{
		//Socket
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		socket.Connect("127.0.0.1", 1234);

		id = socket.LocalEndPoint.ToString();

		//RECV

		socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
		
	}
	private void ReceiveCb(IAsyncResult ar)//接收回调
	{
		//Debug.Log("接收成功");
		try
		{
			int count = socket.EndReceive(ar);
			//数据处理
			string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
			msgList.Add(str);
			//继续接受
			socket.BeginReceive(readBuff, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
		}
		catch(Exception e)
		{
			socket.Close();
		}
		
	}

	void HandleMsg()//处理消息列表
	{
		//获取一条消息
		if (msgList.Count <= 0)
			return;
		string str = msgList[0];
		//Debug.Log(str);
		msgList.RemoveAt(0);
		string[] args = str.Split(' ');
		if(args[0] == "POS")
		{
			OnRecvPos(args[1], args[2], args[3], args[4]);

		}else if(args[0] == "LEAVE")
		{
			OnRecvLeave(args[0]);
		}
	}

	public void OnRecvPos(string id,string xStr,string yStr,string zStr)//处理更新位置的协议
	{
		if(id == this.id)
		{
			return;//不更新自己的位置
		}
		//解析协议
		float x = float.Parse(xStr);
		float y = float.Parse(yStr);
		float z = float.Parse(zStr);

		Vector3 pos = new Vector3(x, y, z);
		if(players.ContainsKey(id))
		{
			players[id].transform.position = pos;
		}else
		{
			AddPlayer(id, pos);
		}
	}

	public void OnRecvLeave(string id)//处理离开的协议
	{
		if(players.ContainsKey(id))
		{
			Destroy(players[id]);
			players[id] = null;
		}
	}
}
