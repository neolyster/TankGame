using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Linq;


public class net : MonoBehaviour {

	ProtocolBase proto = new ProtocolBytes();
	//与服务端的套接字
	
	Socket socket;

	int buffCount = 0;
	byte[] lenBytes = new byte[sizeof(Int32)];
	Int32 msgLength = 0;
	
	
	//服务端的IP和端口

	public InputField hostInput;

	public InputField portInput;

	public InputField textInput;//聊天输入框

	//文本框

	public Text recvText;

	public string recvStr;

	public Text clientText;

	

	//缓冲区

	const int BufferSize = 1024;

	byte[] readBuff = new byte[BufferSize];

	public void Connection()
	{

		recvText.text = "";
		//socket
		socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

		//Connect

		string host = hostInput.text;

		int port = int.Parse(portInput.text);

		socket.Connect(host, port);

		clientText.text = "客户端地址" + socket.LocalEndPoint.ToString();

		//Send

		//string str = "Hello Unity";

		//byte[] bytes = System.Text.Encoding.Default.GetBytes(str);

		//socket.Send(bytes);

		//Recv

		socket.BeginReceive(readBuff, 0, BufferSize, SocketFlags.None,  Receivecb, null);

		//int count = socket.Receive(readBuff);
		//str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
		//recvText.text = str;
		////close
		//socket.Close();

	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		recvText.text = recvStr; 
	}

	private void Receivecb(IAsyncResult ar)
	{
		try
		{
			//count 接收数据的大小
			int count = socket.EndReceive(ar);
			//数据处理
			string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
			buffCount += count;
			ProcessData();

			if(recvStr.Length >300)
			{
				recvStr = "";
			}
			recvStr += str + "\n";
			//继续接收
			socket.BeginReceive(readBuff, 0, BufferSize, SocketFlags.None, Receivecb, null);

		}catch(Exception e)
		{
			recvText.text += "连接已断开";
			socket.Close();
		}
	}
	private void ProcessData()
	{
		if (buffCount < sizeof(Int32))
		{
			return;
		}
		//消息长度
		Array.Copy(readBuff, lenBytes, sizeof(Int32));
		msgLength = BitConverter.ToInt32(lenBytes, 0);
		if (buffCount < msgLength + sizeof(Int32))
		{
			return;
		}
		//处理消息
		ProtocolBase protocol = proto.Decode(readBuff, sizeof(Int32), msgLength);
		string str = System.Text.Encoding.UTF8.GetString(readBuff, sizeof(Int32), msgLength);

		recvStr = str;
		HandleMsg(protocol);
		

		//消除已经处理过的消息

		int count = buffCount - msgLength - sizeof(Int32);

		Array.Copy(readBuff, sizeof(Int32) + msgLength, lenBytes, 0, count);

		buffCount = count;

		if (buffCount > 0)
			ProcessData();

	}

	public void Send()
	{
		string str = textInput.text;
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
		byte[] length = BitConverter.GetBytes(bytes.Length);
		byte[] sendbuff = length.Concat(bytes).ToArray();

		socket.Send(sendbuff);
	}

	public void Send(ProtocolBase protocol)
	{
		byte[] bytes = protocol.Encode();
		byte[] length = BitConverter.GetBytes(bytes.Length);
		byte[] sendbuff = length.Concat(bytes).ToArray();
		socket.Send(sendbuff);
	}

	public void HandleMsg(ProtocolBase protoBase)
	{
		ProtocolBytes proto = (ProtocolBytes)protoBase;
		Debug.Log("接收" + proto.GetDesc());
	}

	public void onSendClick()
	{
		ProtocolBytes protocol = new ProtocolBytes();
		protocol.AddString("HeartBeat");
		Debug.Log("发送" + protocol.GetDesc());
		Send(protocol);
	}
}
