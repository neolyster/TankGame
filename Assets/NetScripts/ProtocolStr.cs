using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ProtocolStr:ProtocolBase
{
	public string str;//传输的字符串
					  //解码器
	public override ProtocolBase Decode(byte[] readbuff, int start, int length)
	{
		ProtocolStr protocol = new ProtocolStr();
		protocol.str = System.Text.Encoding.UTF8.GetString(readbuff, start, length);
		return (ProtocolBase)protocol;
	}

	//解码器
	public override byte[] Encode()
	{
		byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
		return b;
	}

	//协议名称
	public override string GetName()
	{
		if (str.Length == 0)
			return "";
		return str.Split(',')[0];
	}
	//协议描述
	public override string GetDesc()
	{
		return str;
	}

}

