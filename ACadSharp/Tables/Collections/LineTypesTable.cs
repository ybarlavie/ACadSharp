﻿using ACadSharp.IO.Templates;

namespace ACadSharp.Tables.Collections
{
	public class LineTypesTable : Table<LineType>
	{
		public override ObjectType ObjectType => ObjectType.LTYPE_CONTROL_OBJ;

		public LineTypesTable(CadDocument document) : base(document) { }
	}
}