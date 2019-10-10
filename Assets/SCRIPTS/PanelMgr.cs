using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class PanelMgr : MonoBehaviour {


	public static PanelMgr instance;

	private GameObject canvas;

	public GameObject panel1;

	public GameObject panel2;

	public Dictionary<string, PanelBase> dict;//面板

	public Dictionary<PanelLayer, Transform> layerDict;//层级


	
	public void Awake()
	{
		instance = this;
		InitLayer();
		dict = new Dictionary<string, PanelBase>();

	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void onPanel1BtnClick()
	{
		panel1.SetActive(true);
		panel2.SetActive(false);
	}

	public void onPanel2BtnClick()
	{
		panel2.SetActive(true);
		panel1.SetActive(false);
	}

	public void InitLayer()
	{
		canvas = GameObject.Find("Canvas");
		if(canvas == null)
		{
			Debug.LogError("panelMgr.initLayer fail, canvas is null");
		}
		layerDict = new Dictionary<PanelLayer, Transform>();

		int i = 0;
		foreach(PanelLayer pl in Enum.GetValues(typeof(PanelLayer)))
		{
			i++;
			string name = pl.ToString();
			//Debug.Log(name);
			Transform transform = canvas.transform.Find(name);
			layerDict.Add(pl, transform);

		}
		//Debug.Log(i);
	}

	public void OpenPanel<T>(string skinPath, params object[] args) where T : PanelBase//打开面板
	{
		string name = typeof(T).ToString();
		Debug.Log(name);
		if (dict.ContainsKey(name))
		{
			return;
		}
		//面板脚本
		PanelBase panel = canvas.AddComponent<T>();

		panel.Init(args);

		dict.Add(name, panel);

		//加载皮肤

		skinPath = (skinPath != "" ? skinPath : panel.skinPath);
		Debug.Log("skinpath/// "+skinPath);
		GameObject skin = Resources.Load<GameObject>(skinPath);
		if (skin == null)
		{
			Debug.LogError("panelMgr.OpenPanel fail ,skin is null,skinPathj = " + skinPath);
			

		}
		panel.skin = (GameObject)Instantiate(skin);

		//坐标

		Transform skinTrans = panel.skin.transform;

		PanelLayer layer = panel.layer;

		Transform parent = layerDict[layer];

		skinTrans.SetParent(parent,false);

		panel.OnShowing();

		panel.OnShowed();
			 

	}

	public void ClosePanel(string name)
	{
		PanelBase panel = (PanelBase)dict[name];
		if (panel == null)
			return;
		panel.OnClosing();
		dict.Remove(name);
		panel.OnClosed();
		GameObject.Destroy(panel.skin);
		GameObject.Destroy(panel);
	}
}

public enum PanelLayer
{
	Panel,//面板
	Tips,//提示
}

