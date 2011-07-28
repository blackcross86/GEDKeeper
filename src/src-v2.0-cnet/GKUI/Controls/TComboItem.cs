using GKSys;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GKUI.Controls
{
	public class TComboItem
	{
		public string Caption;
		public object Data;

		public TComboItem(string aCaption, object aData)
		{
			this.Caption = aCaption;
			this.Data = aData;
		}

		public override string ToString()
		{
			return this.Caption;
		}

		public void Free()
		{
			TObjectHelper.Free(this);
		}
	}
}
