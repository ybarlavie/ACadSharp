﻿using ACadSharp.Objects;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class DwgDictionaryTemplate : DwgTemplate<CadDictionary>
	{
		public Dictionary<string, ulong> Entries { get; set; } = new Dictionary<string, ulong>();

		public DwgDictionaryTemplate(CadDictionary dictionary) : base(dictionary) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (this.OwnerHandle.HasValue && this.OwnerHandle == 0)
			{
				builder.DocumentToBuild.RootDictionary = this.CadObject;
			}

			foreach (var item in this.Entries)
			{
				if (builder.TryGetCadObject(item.Value, out CadObject entry))
				{
					this.CadObject.Entries.Add(item.Key, entry);
				}
			}
		}
	}
}
