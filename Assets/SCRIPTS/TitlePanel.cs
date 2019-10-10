using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitlePanel : PanelBase {

	private Button startBtn;

	private Button InfoBtn;

#region  生命周期
	public override void Init(params object[] args)
	{
		base.Init(args);
		skinPath = "TitlePanel";
		layer = PanelLayer.Panel;
	}

	public override void OnShowing()
	{
		base.OnShowing();
		Transform skinTrans = skin.transform;
		startBtn = skinTrans.Find("StartBtn").GetComponent<Button>();
		InfoBtn = skinTrans.Find("InfoBtn").GetComponent<Button>();

		startBtn.onClick.AddListener(OnStartClick);
		InfoBtn.onClick.AddListener(OnInfoClick);
	}
	#endregion
	
	public void OnStartClick()
	{
		//Battle.instance.StartTwoCampBattle(2, 2);
		//Close();
		PanelMgr.instance.OpenPanel<OptionPanel>("");
	}

	public void OnInfoClick()
	{
		PanelMgr.instance.OpenPanel<InfoPanel>("");
	}
}
