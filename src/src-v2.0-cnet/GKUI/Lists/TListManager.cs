using GedCom551;
using GKCore;
using GKSys;
using GKUI.Controls;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GKUI.Lists
{
	public abstract class TListManager : IDisposable
	{
		internal TGEDCOMTree FTree;
		protected internal bool Disposed_;

		public TListManager(TGEDCOMTree aTree)
		{
			this.FTree = aTree;
		}

		public void Dispose()
		{
			if (!this.Disposed_)
			{
				this.Disposed_ = true;
			}
		}

		public void UpdateTitles(TGKListView aList, bool isMain)
		{
			try
			{
				aList.Columns.Clear();
				this.UpdateColumns(aList, isMain);
			}
			finally
			{
			}
		}

		public abstract bool CheckFilter(TPersonsFilter aFilter, TGenEngine.TShieldState aShieldState);

		public abstract void Fetch(TGEDCOMRecord aRec);

		public virtual string GetColumnValue(int aColIndex, bool isMain)
		{
			return "";
		}

		public virtual void InitFilter(TPersonsFilter aFilter)
		{
		}

		public abstract void UpdateItem(TExtListItem aItem, bool isMain);

		public abstract void UpdateColumns(TGKListView aList, bool isMain);

		public virtual void GetRow(TGEDCOMRecord aRec, bool isMain, ref string aRow)
		{
			this.Fetch(aRec);
			aRow = TGenEngine.GetId(aRec).ToString();
		}

		public void Free()
		{
			TObjectHelper.Free(this);
		}
	}
}
