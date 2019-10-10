using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

	public float speed = 100f;//炮弹速度

	public GameObject explode;//爆炸特效

	public float maxLiftTime = 2f;//最大持续时间

	public float instantiateTime = 0f;//发射时间

	public GameObject attackTank;

	public AudioClip explodeClip;



	// Use this for initialization
	void Start () {
		instantiateTime = Time.time;

	}
	
	// Update is called once per frame
	void Update () {
		//前进
		transform.position += transform.forward * speed * Time.deltaTime;

		//摧毁

		if (Time.time - instantiateTime > maxLiftTime)
			Destroy(gameObject);
	}

	public void OnCollisionEnter(Collision collision)

	{

		if(collision.gameObject == attackTank)//不小心打到自己不掉血
		{
			return;
		}
		//爆炸效果
		GameObject explodeObj = (GameObject)Instantiate(explode, transform.position, transform.rotation);
		//爆炸音效
		AudioSource audioSource = explodeObj.AddComponent<AudioSource>();
		audioSource.spatialBlend = 1;
		audioSource.PlayOneShot(explodeClip);


		//摧毁自身

		Destroy(gameObject);

		//击中坦克

		Tank tank = collision.gameObject.GetComponent<Tank>();//获取坦克类的信息

		if(tank!=null)
		{
			float dmg = GetDmg();
			tank.BeAttacked(dmg,attackTank);
		}


	}

	private float GetDmg()
	{
		float dmg = 100 - (Time.time - instantiateTime) * 40;
		if (dmg < 1)
			dmg = 1;
		return dmg;
	}
}
