using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

	public Tank tank;

	public GameObject target;//锁定的坦克

	private float sightDistance = 25.0f;//视野范围

	private float lastSearchTime = 0f;//上一次搜索时间

	private float searchTimeInterval = 3f;//搜索间隔

	private Path path ;//路径

	private float lastUpdateWaypointTime = float.MinValue;//上次更新路径时间

	private float updateWaypointInterval = 10;//更新路径cd

	

	public enum Status
	{
		Patrol,
		Attack,
	}
	private Status status = Status.Patrol;

	public void ChangeStatus(Status status)
	{
		if(status == Status.Patrol)
		{
			PatrolStart();

		}else if(status == Status.Attack)
		{
			AttackStart();
		}
	}


	
	// Use this for initialization
	void Start () {
		path = gameObject.AddComponent<Path>();
		Initwaypoint();
		
	}
	
	// Update is called once per frame
	void Update () {

		TargetUpdate();

		if(tank.ctrlType !=Tank.CtrlTyle.computer)
		{
			return;
		}

		if(status == Status.Patrol)
		{
			PatrolUpdate();
			
		}else if(status == Status.Attack)
		{
			AttackUpdate();
		}
		if(path.isReached(transform))
		{
			path.NextWaypoint();
		}
		
		
	}

	void AttackStart()//攻击开始
	{
		Vector3 targetPos = target.transform.position;

		path.InitByNavMeshPath(transform.position, targetPos);

	}
	void PatrolStart()//巡逻开始
	{


	}

	void PatrolUpdate()//巡逻逻辑
	{
		if (target != null)
			ChangeStatus(Status.Attack);
		float interval = Time.time - lastUpdateWaypointTime;
		if (interval < updateWaypointInterval)
			return;
		if(path.waypoints == null || path.isFinished)//处理巡逻点
		{
			GameObject obj = GameObject.Find("WayPointContainer");
			int count = obj.transform.childCount;
			int index = Random.Range(0, count);
			if (count == 0)
				return;
			Vector3 targetPos = obj.transform.GetChild(index).position;
			path.InitByNavMeshPath(transform.position, targetPos);
		}
	}

	void AttackUpdate()//攻击逻辑
	{
		if (target == null)
			ChangeStatus(Status.Attack);//目标丢失切换状态
		float interval = Time.time - lastUpdateWaypointTime;
		if (interval < updateWaypointInterval)
			return;
		lastUpdateWaypointTime = Time.time;

		Vector3 targetPos = target.transform.position;
		path.InitByNavMeshPath(transform.position, targetPos);


	}

	void TargetUpdate()//搜寻目标
	{
		//cd时间
		float interval = Time.time - lastSearchTime;

		if(interval <searchTimeInterval)
		{
			return;
		}
		if (target != null)
		{
			HasTarget();
		}
		else
			NoTarget();
	}

	void HasTarget()
	{
		Tank targetTank = target.GetComponent<Tank>();
		Vector3 pos = transform.position;
		Vector3 targetPos = target.transform.position;

		if(targetTank.ctrlType == Tank.CtrlTyle.none)
		{
			Debug.Log("目标死亡");
			target = null;
		}else if(Vector3.Distance(pos,targetPos)>sightDistance)
		{
			Debug.Log("丢失目标");
			target = null;
		}
	}
	//如果没有目标 则开始搜索目标
	void NoTarget()
	{
		float minHp = float.MaxValue;//最小生命值

		GameObject[] targets = GameObject.FindGameObjectsWithTag("Tank");

		for(int i = 0;i<targets.Length;i++)
		{
			Tank tank = targets[i].GetComponent<Tank>();

			if (tank == null)
				continue;
			if (targets[i] == gameObject)//如果是自己则判断下一个

				continue;

			//队友
			if (Battle.instance.IsSameCamp(gameObject, targets[i]))
				continue;
			//死亡
			if (tank.ctrlType == Tank.CtrlTyle.none)
				continue;

			Vector3 pos = gameObject.transform.position;

			Vector3 targetPos = targets[i].transform.position;

			if (Vector3.Distance(pos, targetPos) > sightDistance)
				continue;
			if (minHp > tank.hp)
				target = tank.gameObject;
		}

		if (target != null)
			Debug.Log("获取目标"+ target.name);

	}

	public void OnAttack(GameObject attackTank)//被别的坦克攻击 自动还击
	{
		//队友误伤
		if (Battle.instance.IsSameCamp(attackTank, gameObject))
			return;
		target = attackTank;
	}

	public Vector3 getTurretTarget()
	{
		if(target == null)
		{
			float y = transform.eulerAngles.y;
			Vector3 rot = new Vector3(0, y, 0);
			return rot;
		}else
		{
			Vector3 pos = transform.position;
			Vector3 targetPos = target.transform.position;
			Vector3 vec = targetPos - pos;
			return Quaternion.LookRotation(vec).eulerAngles;
		}
	}

	public bool IsShoot()
	{
		if (target == null)
			return false;
		float turretRoll = tank.turret.eulerAngles.y;
		float angle = turretRoll - getTurretTarget().y;

		if (angle < 0)
			angle += 360;
		if (angle < 30 || angle > 330)
			return true;
		else
			return false;
	}
	
	void Initwaypoint()//初始化路径点
	{
		GameObject obj = GameObject.Find("WayPointContainer");
		if (obj && obj.transform.GetChild(0) != null)
		{
			Vector3 targetPos = obj.transform.GetChild(0).position;

			path.InitByNavMeshPath(transform.position, targetPos);
		}
	}

	void OnDrawGizmos()//在AI类中编制
	{
		path.DrawWaypoints();
	}

	public float GetSteering()//转向
	{
		if (tank == null)
			return 0;
		Vector3 itp = transform.InverseTransformPoint(path.waypoint);
		if (itp.x > path.deviation / 5)//左转
			return tank.maxSteeringAngle;
		else if (itp.x < -path.deviation / 5)
			return -tank.maxSteeringAngle;//右转
		else
			return 0;
	}

	public float GetMotor()//获取马力
	{
		if (tank == null)
			return 0;

		Vector3 itp = transform.InverseTransformPoint(path.waypoint);
		float x = itp.x;
		float z = itp.z;
		float r = 6;

		if (z < 0 && Mathf.Abs(x) < -z && Mathf.Abs(x) < r)
		{
			return -tank.maxMotorTorque;
		}
		else
			return tank.maxMotorTorque;
			
	}

	public float GetBrakeTorque()//刹车
	{
		if (path.isFinished)
		{
			return tank.maxBrakeTorque;
		}
		else
			return 0;
	}
}
