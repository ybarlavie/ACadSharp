using ACadSharp.Entities;
using ACadSharp.IO;
using ACadSharp.IO.DWG;
using ACadSharp.IO.DXF;
using ACadSharp.Tables;
using System;
using System.Collections.Generic;
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

		static string safeJsonString(string s)
		{
			if (string.IsNullOrEmpty(s)) return "";
			return s.Replace("\"", "\\\"");
		}

		static void ReadDwg()
		{
			//string file = "../samples/dwg/PYLOT_2013.dwg";
			//string file = "../samples/local/Teura_Exchange_313.dwg";
			string file = "..\\..\\..\\..\\samples\\local\\Teura_Exchange_313.dwg";

			StringBuilder sb = new StringBuilder();

			using (DwgReader reader = new DwgReader(file, onNotification))
			{
				CadDocument doc = reader.Read();

				Dictionary<string, string[]> blocksFields = new Dictionary<string, string[]>(); 
				// build block fields dictionary
				foreach (BlockRecord br in doc.BlockRecords) 
				{
					if (br.Name != "*Model_Space" && br.Name != "*Paper_Space" && br.ObjectName == "BLOCK_RECORD") 
					{
						List<string> names = new List<string>();
						foreach(Entity ent in br.Entities)
						{
							if (ent.ObjectName == "ATTDEF") 
							{
								if (!string.IsNullOrEmpty(((AttributeDefinition)ent).Tag))
								{
									names.Add(((AttributeDefinition)ent).Tag);
								} 
								else if (!string.IsNullOrEmpty(((AttributeDefinition)ent).Prompt))
								{
									names.Add(((AttributeDefinition)ent).Prompt);
								}
							}
						}
						blocksFields.Add(br.Name, names.ToArray());
					}
				}

				sb.AppendLine("{\"type\": \"FeatureCollection\", \"features\": [");

				bool isFirst = true;
				foreach (BlockRecord br in doc.BlockRecords)
				{
					if (br.Name == "*Model_Space" && br.ObjectName == "BLOCK_RECORD") 
					{
						Console.WriteLine($"extracted {br.ObjectName}:{br.Name}");
						foreach (CadObject ent in br.Entities)
						{
							if (ent.ObjectName != "INSERT")
							{
								continue;
							}
							Insert ins = ((Insert)ent);
							BlockRecord br2 = ins.Block;

							if (!isFirst) sb.AppendLine(", ");

							sb.Append("{");
							sb.Append($" \"type\": \"Feature\", \"id\": \"{ins.Handle}\", ");

							sb.Append(" \"geometry\": { ");
							sb.Append($" \"type\": \"Point\", \"coordinates\": [ {ins.InsertPoint.X}, {ins.InsertPoint.Y} ] ");
							sb.Append("}, ");

							sb.Append(" \"properties\": { ");

							bool isFirstAtt = true;

							AttributeEntity[] atts = ins.Attributes.ToArray();
							for (int i=0; i<atts.Length; i++)
							{
								AttributeEntity att = atts[i];
								

								string propName = safeJsonString(blocksFields[br2.Name][i]);
								if (!string.IsNullOrEmpty(propName)) 
								{
									if (!isFirstAtt) sb.AppendLine(", ");

									string propVal = safeJsonString(att.Value);

									sb.Append($" \"{propName}\": \"{propVal}\" ");

									isFirstAtt = false;
								}
							}

							if (!isFirstAtt) sb.AppendLine(", ");
							sb.Append($" \"Layer\": \"{ins.Layer.Name}\" ");

							sb.Append("} ");

							sb.Append("}");
							isFirst = false;
						}
					}
				}

				sb.AppendLine("]}");

				string jsonName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + "_points.geojson");
				File.WriteAllText(jsonName, sb.ToString());
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
