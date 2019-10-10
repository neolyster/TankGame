using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PanelBase : MonoBehaviour {

	public string skinPath;

	public GameObject skin;

	public object[] args;//参数

	public PanelLayer layer;

	#region 生命周期

	public virtual void Init(params object[] args)//初始化
	{
		this.args = args;
	}

	public virtual void OnShowing()
	{

	}

	public virtual void OnShowed()
	{

	}

	public virtual void Update()
	{

	}
	//关闭前
	public virtual void OnClosing()
	{

	}
	//关闭后
	public virtual void OnClosed()
	{

	}
	#endregion

	#region 操作
	protected virtual void Close()
	{
		string name = this.GetType().ToString();
		PanelMgr.instance.ClosePanel(name);
	}
	#endregion
}
