using GKSys;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GKCore
{
	public class TPedigreeOptions
	{
		public enum TPedigreeFormat : byte { pfExcess, pfCompact }

		private TPedigreeOptions.TPedigreeFormat FFormat;
		private bool FIncludeNotes;
		private bool FIncludeAttributes;
		private bool FIncludeSources;

		public TPedigreeOptions.TPedigreeFormat Format
		{
			get { return this.FFormat; }
			set { this.FFormat = value; }
		}

		public bool IncludeAttributes
		{
			get { return this.FIncludeAttributes; }
			set { this.FIncludeAttributes = value; }
		}

		public bool IncludeNotes
		{
			get { return this.FIncludeNotes; }
			set { this.FIncludeNotes = value; }
		}

		public bool IncludeSources
		{
			get { return this.FIncludeSources; }
			set { this.FIncludeSources = value; }
		}

		public TPedigreeOptions()
		{
			this.FIncludeAttributes = true;
			this.FIncludeNotes = true;
			this.FIncludeSources = true;
		}

		public void LoadFromFile([In] TIniFile aIniFile)
		{
			this.FIncludeAttributes = aIniFile.ReadBool("Pedigree", "IncludeAttributes", true);
			this.FIncludeNotes = aIniFile.ReadBool("Pedigree", "IncludeNotes", true);
			this.FIncludeSources = aIniFile.ReadBool("Pedigree", "IncludeSources", true);
			this.FFormat = (TPedigreeOptions.TPedigreeFormat)aIniFile.ReadInteger("Pedigree", "Format", 0);
		}

		public void SaveToFile([In] TIniFile aIniFile)
		{
			aIniFile.WriteBool("Pedigree", "IncludeAttributes", this.FIncludeAttributes);
			aIniFile.WriteBool("Pedigree", "IncludeNotes", this.FIncludeNotes);
			aIniFile.WriteBool("Pedigree", "IncludeSources", this.FIncludeSources);
			aIniFile.WriteInteger("Pedigree", "Format", (int)((sbyte)this.FFormat));
		}

		public void Free()
		{
			TObjectHelper.Free(this);
		}
	}
}
