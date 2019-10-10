using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle : MonoBehaviour {

	

	public static Battle instance;//单例模式

	public BattleTank[] battleTanks;

	public GameObject[] tankPrefabs;

	// Use this for initialization
	void Start () {
		instance = this;
		//StartTwoCampBattle(2, 1);
	}
	
	// Update is called once per frame
	void Update () {
		//Debug.Log(battleTanks.Length + "battletanks length");
	}

	public int GetCamp(GameObject tankObj)//返回0表示错误
	{
		//Debug.Log(battleTanks.Length + "length");
		for(int i =0;i<battleTanks.Length;i++)
		{
			Debug.Log("i = " + i+"battletanks length = "+ battleTanks.Length);
			BattleTank battleTank = battleTanks[i];
			Debug.Log(battleTank.tank.name + " /" + battleTank.camp);
			if (battleTanks == null)
			{
				
				
				return 0;
			}
			if (battleTank.tank.gameObject == tankObj)
			{
				Debug.Log("yes");
				return battleTanks[i].camp;
				
			}
		}
		return 0;
	}

	public bool IsSameCamp(GameObject tank1,GameObject tank2)//判断2者是否同一阵营
	{
		return GetCamp(tank1) == GetCamp(tank2);
	}

	public bool IsWin(int camp)//判断胜负
	{
		for (int i = 0; i < battleTanks.Length; i++)
		{

			Tank tank = battleTanks[i].tank;
			if (battleTanks[i].camp != camp)
			{
				if (tank.hp > 0)
					return false;
			}
		}
		Debug.Log("阵营" + camp + "获胜");

		PanelMgr.instance.OpenPanel<WinPanel>("", camp);
		return true;
	}

	public bool IsWin(GameObject obj)
	{
		Debug.Log("iswin(obj)");
		int camp = GetCamp(obj);
		return IsWin(camp);
	}


	public void ClearBattle()
	{
		GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tank");
		for(int i =0;i<tanks.Length;i++)
		{
			Destroy(tanks[i]);
		}
	}

	public void StartTwoCampBattle(int n1,int n2)
	{
		Transform sp = GameObject.Find("SwopPoints").transform;

		Transform sp1 = sp.GetChild(0);

		Transform sp2 = sp.GetChild(1);

		if(sp1.childCount <n1||sp2.childCount <n2)
		{
			Debug.Log("出生点数量不足");
			return;
		}

		if(tankPrefabs.Length < 2)
		{
			Debug.Log("坦克预设数量不足");
			return;
		}

		ClearBattle();

		battleTanks = new BattleTank[n1 + n2];
		for(int i=0;i<n1;i++)
		{
			GererateTank(1, i, sp1, i);
			
		}
		for(int i = 0;i<n2;i++)
		{
			GererateTank(2, i, sp2, n1+i);
		}

		Tank tankCmp = battleTanks[0].tank;//第一辆坦克玩家操控
		tankCmp.ctrlType = Tank.CtrlTyle.player;
		Debug.Log("坦克数量" + battleTanks.Length);

		//设置相机

		CameraFollow cf = Camera.main.gameObject.GetComponent<CameraFollow>();

		GameObject target = tankCmp.gameObject;
		cf.SetTarget(target);
	}

	public void GererateTank(int camp,int number,Transform sp,int index)
	{
		//获取出生点和预设

		Transform trans = sp.GetChild(number);

		Vector3 pos = trans.position;

		Quaternion rot = trans.rotation;

		GameObject prefab = tankPrefabs[camp - 1];

		GameObject tankobj = (GameObject)Instantiate(prefab, pos, rot);//产生坦克

		//设置坦克属性
		Tank tankcmp = tankobj.GetComponent<Tank>();

		tankcmp.ctrlType = Tank.CtrlTyle.computer;

		battleTanks[index] = new BattleTank();

		battleTanks[index].tank = tankcmp;

		battleTanks[index].camp = camp;
		Debug.Log(camp + " /" + tankobj.name);


	}
}
