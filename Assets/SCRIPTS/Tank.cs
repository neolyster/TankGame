using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tank : MonoBehaviour
{
    //炮塔炮管轮子履带
    public Transform turret;
    public Transform gun;
    private Transform wheels;
    private Transform tracks;
    //炮塔旋转速度
    private float turretRotSpeed = 0.5f;
    //炮塔炮管目标角度
    private float turretRotTarget = 0;
    private float turretRollTarget = 0;
    //炮管的旋转范围
    private float maxRoll = 10f;
    private float minRoll = -4f;

	//中心准心
	public Texture2D centerSight;
	//坦克准心
	public Texture2D tankSight;

	//生命指示条素材
	public Texture2D hpBarBg;
	public Texture2D hpBar;
	//击杀素材
	public Texture2D killbar;
	private float killUIStartTime = float.MinValue;

	//发射炮弹音源
	public AudioSource shootAudioSource;

	//发射炮弹音效

	public AudioClip shootClip;

    //轮轴
    public List<AxleInfo> axleInfos;
    //马力/最大马力
    private float motor = 0;
    public float maxMotorTorque;
    //制动/最大制动
    private float brakeTorque = 0;
    public float maxBrakeTorque = 100;
    //转向角/最大转向角
    private float steering = 0;
    public float maxSteeringAngle;

	private AI ai;


    //马达音源
    public AudioSource motorAudioSource;
    //马达音效
    public AudioClip motorClip;

	public GameObject bullet;//炮弹

	public float lastShootTime = 0;//上次开炮时间

	public float shootInterval = 0.5f;//开炮间隔

	//生命值

	public float maxHp = 100;

	public float hp = 100;//当前生命值

	public GameObject DestroyEffect;//摧毁特效


	

	public enum CtrlTyle
	{
		none,
		player,
		computer
	}

	public CtrlTyle ctrlType = CtrlTyle.player;

	public void TargetSignPos()//计算目标角度

	{
		Vector3 hitPoint = Vector3.zero;//屏幕中心
		RaycastHit raycastHit;//碰撞点

		Vector3 centerVec = new Vector3(Screen.width / 2, Screen.height / 2,0);
		Ray ray = Camera.main.ScreenPointToRay(centerVec);
		if(Physics.Raycast(ray,out raycastHit,400.0f))//射线检测 获取hitPoint
		{
			hitPoint = raycastHit.point;
		}else
		{
			hitPoint = ray.GetPoint(400);
		}

		//计算目标角度
		Vector3 dir = hitPoint - turret.position;
		Quaternion angle = Quaternion.LookRotation(dir);
		turretRotTarget = angle.eulerAngles.y;
		turretRollTarget = angle.eulerAngles.x;
		//测试目视的点在哪里
		//Transform targetcube = GameObject.Find("TargetCube").transform;
		//targetcube.position = hitPoint;





	}

	public Vector3 CalExplodePoint()//计算爆炸位置
	{
		Vector3 hitPoint = Vector3.zero;//屏幕中心
		RaycastHit raycastHit;//碰撞点

		Vector3 pos = gun.position + gun.forward * 5;
		Ray ray = new Ray(pos, gun.forward);
		if (Physics.Raycast(ray, out raycastHit, 400.0f))//射线检测 获取hitPoint
		{
			hitPoint = raycastHit.point;
		}
		else
		{
			hitPoint = ray.GetPoint(400);
		}

		//Transform targetCube = GameObject.Find("explodeCube").transform;
		//targetCube.position = hitPoint; //测试用

		return hitPoint;

	}

	public void DrawSight()//绘制准心
	{
		Vector3 explodePoint = CalExplodePoint();//实际射击的位置

		Vector3 screenPoint = Camera.main.WorldToScreenPoint(explodePoint);//射击准心的屏幕位置

		Rect tankRect = new Rect(screenPoint.x - tankSight.width / 2, Screen.height - screenPoint.y - tankSight.height / 2, tankSight.width, tankSight.height);//绘制准心位置 因为上面返回值和屏幕绘制坐标系不同 所以要再计算一下

		GUI.DrawTexture(tankRect, tankSight);

		//绘制中心准心

		Rect centerRect = new Rect(Screen.width / 2 - centerSight.width / 2, Screen.height / 2 - centerSight.height / 2, centerSight.width, centerSight.height);

		GUI.DrawTexture(centerRect, centerSight);



		
	}

	public void DrawHp()
	{

		//底框
		Rect bgRect = new Rect(30, Screen.height - hpBarBg.height - 15, hpBarBg.width, hpBarBg.height);

		GUI.DrawTexture(bgRect, hpBarBg);

		//指示条

		float width = hp * 102 / maxHp;

		Rect hpRect = new Rect(bgRect.x + 29, bgRect.y + 9, width, hpBar.height);

		GUI.DrawTexture(hpRect, hpBar);

		//文字

		string text = Mathf.Ceil(hp).ToString() + "/" + Mathf.Ceil(maxHp).ToString();

		Rect textRect = new Rect(bgRect.x + 80, bgRect.y - 10, 50, 50);

		GUI.Label(textRect, text);




	}//显示生命值

	public void StartDrawKill()
	{
		killUIStartTime = Time.time;
	}

	public void DrawKillUI()
	{
		if(Time.time - killUIStartTime<1f)
		{
			Rect rect = new Rect(Screen.width / 2 - killbar.width / 2, 30, killbar.width, killbar.height);
			GUI.DrawTexture(rect, killbar);
		}
	}

	public void Shoot()
	{

		if(Time.time - lastShootTime <shootInterval)//发射间隔
		{
			return;
		}
		//子弹
		if (bullet == null)
			return;

		//发射

		Vector3 pos = gun.position + gun.forward * 5;

		GameObject bulletObj = (GameObject)Instantiate(bullet, pos, gun.rotation);

		Bullet bulletCmp = bulletObj.GetComponent<Bullet>();

		if (bulletObj != null)
			
			bulletCmp.attackTank = this.gameObject;





		

		lastShootTime = Time.time;

		shootAudioSource.PlayOneShot(shootClip);

		//BeAttacked(40);
	}



	public void BeAttacked(float dmg,GameObject attackTank)
	{
		if (hp <= 0)
			return;
		if(hp >0 )
		{
			hp -= dmg;
			if(ai!=null)
			{
				ai.OnAttack(attackTank);
				Debug.Log("ai被打");
			}
		}
		if(hp <= 0)//被摧毁
		{
			GameObject destoryObj = (GameObject)Instantiate(DestroyEffect);
			destoryObj.transform.SetParent(transform, false);//false 代表不受父对象的影响，保留原有的坐标和缩放
			DestroyEffect.transform.localPosition = Vector3.zero;
			ctrlType = CtrlTyle.none;
			if(attackTank != null)
			{
				Tank tankcmp = attackTank.GetComponent<Tank>();

				if(tankcmp != null&&tankcmp.ctrlType == CtrlTyle.player)
				{
					tankcmp.StartDrawKill();
				}
			}

			//战场结算

			Battle.instance.IsWin(attackTank);
		}
	}

    //玩家控制
    public void PlayerCtrl()


    {

		if (ctrlType != CtrlTyle.player)//如果不是玩家控制就返回

			return;
        //马力和转向角
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        //制动
        brakeTorque = 0;
        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.leftWheel.rpm > 5 && motor < 0)  //前进时，按下“下”键
                brakeTorque = maxBrakeTorque;
            else if (axleInfo.leftWheel.rpm < -5 && motor > 0)  //后退时，按下“上”键
                brakeTorque = maxBrakeTorque;
            continue;
        }
		//炮塔炮管角度
		//turretRotTarget = Camera.main.transform.eulerAngles.y;
		//turretRollTarget = Camera.main.transform.eulerAngles.x;
		TargetSignPos();

		if(Input.GetMouseButton(0))//鼠标左键发射
		{
			Shoot();
		}
    }

	public void ComputerCtrl()//电脑控制
	{
		if (ctrlType != CtrlTyle.computer)
			return;
		//炮塔目标角度
		Vector3 rot = ai.getTurretTarget();

		turretRotTarget = rot.y;

		turretRollTarget = rot.x;

		//移动

		steering = ai.GetSteering();

		motor = ai.GetMotor();

		brakeTorque = ai.GetBrakeTorque();

		if (ai.IsShoot())
			Shoot();
	}


	public void NoneCtrl()
	{
		if(ctrlType != CtrlTyle.none)
		{
			return;
		}
		motor = 0;
		steering = 0;
		brakeTorque = maxBrakeTorque / 2;
	}



    //开始时执行
    void Start()
    {
        //获取炮塔
        turret = transform.Find("turret");
        //获取炮管
        gun = turret.Find("gun");
        //获取轮子
        wheels = transform.Find("wheels");
        //获取履带
        tracks = transform.Find("tracks");
        //马达音源
        motorAudioSource = gameObject.AddComponent<AudioSource>();
        motorAudioSource.spatialBlend = 1;

		//发射音源
		shootAudioSource = gameObject.AddComponent<AudioSource>();

		shootAudioSource.spatialBlend = 1;
		//只能
		if(ctrlType == CtrlTyle.computer)
		{
			ai = gameObject.AddComponent<AI>();
			ai.tank = this;
		}

	}

    //每帧执行一次
    void Update()
    {
        //玩家控制操控
        PlayerCtrl();
		ComputerCtrl();//计算机操控
		NoneCtrl();//无人操控
		//遍历车轴
		foreach (AxleInfo axleInfo in axleInfos)
        {
            //转向
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            //马力
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            //制动
            if (true)
            {
                axleInfo.leftWheel.brakeTorque = brakeTorque;
                axleInfo.rightWheel.brakeTorque = brakeTorque;
            }
            //转动轮子履带
            if (axleInfos[1] != null && axleInfo == axleInfos[1])
            {
                WheelsRotation(axleInfos[1].leftWheel);
                TrackMove();
            }
        }

        //炮塔炮管旋转
        TurretRotation();
        TurretRoll();
        //马达音效
        MotorSound();
		CalExplodePoint();

	}

	private void OnGUI()//绘图
	{
		if (ctrlType != CtrlTyle.player)
			return;
		DrawSight();
		DrawHp();
		DrawKillUI();
	}

	//炮塔旋转
	public void TurretRotation()
    {
        if (Camera.main == null)
            return;
        if (turret == null)
            return;

        //归一化角度
        float angle = turret.eulerAngles.y - turretRotTarget;
        if (angle < 0) angle += 360;

        if (angle > turretRotSpeed && angle < 180)
            turret.Rotate(0f, -turretRotSpeed, 0f);
        else if (angle > 180 && angle < 360 - turretRotSpeed)
            turret.Rotate(0f, turretRotSpeed, 0f);
    }

    //炮管旋转
    public void TurretRoll()
    {
        if (Camera.main == null)
            return;
        if (turret == null)
            return;
        //获取角度
        Vector3 worldEuler = gun.eulerAngles;
        Vector3 localEuler = gun.localEulerAngles;
        //世界坐标系角度计算
        worldEuler.x = turretRollTarget;
        gun.eulerAngles = worldEuler;
        //本地坐标系角度限制
        Vector3 euler = gun.localEulerAngles;
        if (euler.x > 180)
            euler.x -= 360;

        if (euler.x > maxRoll)
            euler.x = maxRoll;
        if (euler.x < minRoll)
            euler.x = minRoll;
        gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);
		
    }

    //轮子旋转
    public void WheelsRotation(WheelCollider collider)
    {
        if (wheels == null)
            return;
        //获取旋转信息
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
        //旋转每个轮子
        foreach (Transform wheel in wheels)
        {
            wheel.rotation = rotation;
        }
    }


    //履带滚动
    public void TrackMove()
    {
        if (tracks == null)
            return;

        float offset = 0;
        if (wheels.GetChild(0) != null)
            offset = wheels.GetChild(0).localEulerAngles.x / 90f;

        foreach (Transform track in tracks)
        {
            MeshRenderer mr = track.gameObject.GetComponent<MeshRenderer>();
            if (mr == null) continue;
            Material mtl = mr.material;
            mtl.mainTextureOffset = new Vector2(0, offset);
        }
    }

    //马达音效
    void MotorSound()
    {
        if (motor != 0 && !motorAudioSource.isPlaying)
        {
            motorAudioSource.loop = true;
            motorAudioSource.clip = motorClip;
            motorAudioSource.Play();
        }
        else if (motor == 0)
        {
            motorAudioSource.Pause();
        }
    }
}