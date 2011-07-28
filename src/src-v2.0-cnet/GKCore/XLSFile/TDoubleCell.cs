using GKSys;
using System;

namespace XLSFile
{
	public class TDoubleCell : TCell
	{
		public double Value;

		public TDoubleCell()
		{
			this.opCode = 3;
		}

		public override void Write(TBIFFWriter W)
		{
			base.Write(W);
			W.WriteDouble(this.Value);
		}
	}
}
