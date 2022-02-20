using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DXF;
using ACadSharp.Tables;
using System;
using System.IO;
using System.Text;

namespace ACadSharp.Examples
{
	class Program
	{
		static string PathSamples = "../samples";

		static void Main(string[] args)
		{
			//ReadDxf();
			ReadDwg();
		}

		static void ReadDxf()
		{
			string file = Path.Combine(PathSamples, "dxf/ascii.dxf");
			DxfReader reader = new DxfReader(file);
			reader.Read();
		}

		static void ReadDwg()
		{
			string file = "../samples/dwg/5177.dwg";

			StringBuilder sb = new StringBuilder();

			using (DwgReader reader = new DwgReader(file, onNotification))
			{
				CadDocument doc = reader.Read();

				sb.AppendLine("{\"type\": \"FeatureCollection\", \"features\": [");

				bool isFirst = true;
				foreach (BlockRecord br in doc.BlockRecords)
				{
					if (br.Name == "*Model_Space" && br.ObjectName == "BLOCK_RECORD") 
					{
						Console.WriteLine($"extracted {br.ObjectName}:{br.Name}");
						foreach (CadObject insert in br.Entities)
						{
							Console.WriteLine($"	extracted {insert.ObjectName}:{br.Name}");

							Insert ins = ((Insert)insert);
							
							if (!isFirst) sb.AppendLine(", ");

							sb.Append("{");
								sb.Append($" \"type\": \"Feature\", \"id\": \"{ins.Handle}\", ");

								sb.Append(" \"geometry\": { ");
									sb.Append($" \"type\": \"Point\", \"coordinates\": [ {ins.InsertPoint.X}, {ins.InsertPoint.Y} ] ");
								sb.Append("}, ");

								sb.Append(" \"properties\": { ");

								bool isFirstAtt = true;
								foreach (AttributeEntity ae in ins.Attributes)
								{
									if (!isFirstAtt) sb.AppendLine(", ");
									sb.Append($"\"{ae.Tag}\": \"{ae.Value}\"");
									isFirstAtt = false;
								}
								sb.Append("} ");

							sb.Append("}");
							isFirst = false;
						}
					}
				}

				sb.AppendLine("]}");

				Console.Write(sb.ToString());
			}

			// string[] files = Directory.GetFiles(PathSamples + "/dwg/", "*.dwg");

			// foreach (var f in files)
			// {
			// 	using (DwgReader reader = new DwgReader(f, onNotification))
			// 	{
			// 		CadDocument doc = reader.Read();
			// 	}

			// 	Console.WriteLine($"file read : {f}");
			// 	//Console.ReadLine();
			// }
		}

		private static void onNotification(object sender, NotificationEventArgs e)
		{
			Console.WriteLine(e.Message);
		}
	}
}
