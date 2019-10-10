using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Path : MonoBehaviour {

	public Vector3[] waypoints;//所有路径点

	public int index = -1;

	public Vector3 waypoint;

	public bool isLoop = false;

	public float deviation = 5;//到达误差

	public bool isFinished = false;

	public bool isReached(Transform trans)
	{

		Vector3 pos = trans.position;
		float distance = Vector3.Distance(waypoint, pos);
		
		return distance < 5;
	}

	public void NextWaypoint()//寻找下一个目标点
	{
		if (index < 0)
			return;
		if(index < waypoints.Length -1)
		{
			index++;

		}else
		{
			if (isLoop)
				index = 0;
			else
				isFinished = true;
		}
		waypoint = waypoints[index];
		Debug.Log("next");
		
	}

	public void InitByObj(GameObject obj,bool isLoop = false)
	{
		int length = obj.transform.childCount;

		if(length == 0)
		{
			waypoints = null;
			index = -1;
			Debug.LogWarning("Path.InitByObj length == 0");
			return;

		}

		//遍历子物体生成路径点

		waypoints = new Vector3[length];

		for( int i =0;i<length;i++)
		{
			Transform trans = obj.transform.GetChild(i);
			waypoints[i] = trans.position;
		}

		index = 0;
		waypoint = waypoints[index];
		this.isLoop = true;
		isFinished = false;
		
		
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void InitByNavMeshPath(Vector3 pos,Vector3 targetPos)//根据导航图初始化路径
	{
		waypoints = null;
		index = -1;
		NavMeshPath navPath = new NavMeshPath();
		bool hasFoundPath = NavMesh.CalculatePath(pos, targetPos, NavMesh.AllAreas, navPath);

		if(!hasFoundPath)
		{
			return;
		}
		int length = navPath.corners.Length;//生成路径
		waypoints = new Vector3[length];
		for(int i =0;i<length;i++)
		{
			waypoints[i] = navPath.corners[i];
		}

		index = 0;
		waypoint = waypoints[index];
		isFinished = false;


	}
	public void DrawWaypoints()//调试路径
	{
		if (waypoints == null)
			return;
		int length = waypoints.Length;
		for(int i = 0;i<length;i++)
		{
			if (i == index)
				Gizmos.DrawSphere(waypoints[i], 1);//以球形标记当前路点
			else
				Gizmos.DrawCube(waypoints[i], Vector3.one);//以方块标记其他路点
		}

	}
}
