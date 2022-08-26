using System.Text;
using System.Xml;

class Program
{
	static string workPath = @".";

	static XmlWriterSettings xws = new XmlWriterSettings();

	static int CheckAddRepeatNode(XmlNode ele, XmlNode node, XmlDocument doc)
	{
		XmlAttribute? typeAttr = ele.Attributes["type"];
		if (typeAttr == null) return 0;

		XmlAttribute? repeatAttr = ele.Attributes["repeat"]; 
		if (repeatAttr == null) return 0;

		XmlAttribute? postfixAttr = ele.Attributes["postfix"];

		XmlNode? parentNode = node.ParentNode;
		if (parentNode == null) return 0;

		String postfix = postfixAttr?.Value ?? "List";

		int nodeVersion = int.Parse(node.Attributes["version"]?.Value ?? "1");
		int eleVersion = int.Parse(ele.Attributes["version"]?.Value ?? "1");
		int addVersion = Math.Max(nodeVersion, eleVersion);

		String addName = typeAttr.Value + postfix;

		XmlNode addNode = doc.CreateNode(XmlNodeType.Element, "struct", null);
		parentNode.AppendChild(addNode);
		parentNode.InsertBefore(addNode, node);

		XmlAttribute addNameAttr = doc.CreateAttribute("name");
		addNameAttr.Value = addName;
		addNode.Attributes?.Append(addNameAttr);

		XmlAttribute addVersionAttr = doc.CreateAttribute("version");
		addVersionAttr.Value = addVersion.ToString();
		addNode.Attributes?.Append(addVersionAttr);

		//ADD Count
		XmlNode addCountNode = doc.CreateNode(XmlNodeType.Element, "entry", null);
		addNode.AppendChild(addCountNode);

		XmlAttribute addCountNameAttr = doc.CreateAttribute("name");
		addCountNameAttr.Value = "Count";
		addCountNode.Attributes?.Append(addCountNameAttr);

		XmlAttribute addCountTypeAttr = doc.CreateAttribute("type");
		addCountTypeAttr.Value = "int";
		addCountNode.Attributes?.Append(addCountTypeAttr);

		//ADD Data
		XmlNode addDataNode = doc.CreateNode(XmlNodeType.Element, "entry", null);
		addNode.AppendChild(addDataNode);

		XmlAttribute addDataNameAttr = doc.CreateAttribute("name");
		addDataNameAttr.Value = "Data";
		addDataNode.Attributes?.Append(addDataNameAttr);

		XmlAttribute addDataTypeAttr = doc.CreateAttribute("type");
		addDataTypeAttr.Value = typeAttr.Value;
		addDataNode.Attributes?.Append(addDataTypeAttr);

		XmlAttribute addDataReferAttr = doc.CreateAttribute("refer");
		addDataReferAttr.Value = "Count";
		addDataNode.Attributes?.Append(addDataReferAttr);

		XmlAttribute addDataCountAttr = doc.CreateAttribute("count");
		addDataCountAttr.Value = repeatAttr.Value;
		addDataNode.Attributes?.Append(addDataCountAttr);

		typeAttr.Value = addName;
		ele.Attributes.Remove(repeatAttr);

		if (postfixAttr != null)
		{
			ele.Attributes.Remove(postfixAttr);
		}

		return 1;
	}

	static void HandleXml(string xmlName)
	{
		XmlDocument doc = new XmlDocument();
		doc.PreserveWhitespace = true;
		doc.Load(xmlName);

		XmlNodeList nodeList = doc.GetElementsByTagName("struct");

		for (int i = 0; i < nodeList.Count; i++)
		{
			XmlNode node = nodeList[i];

			foreach (XmlNode ele in node)
			{
				if (ele.NodeType != XmlNodeType.Element)
				{
					continue;
				}

				int iAddCount = CheckAddRepeatNode(ele, node, doc);
				if (iAddCount <= 0)
				{
					continue;
				}

				if (iAddCount > 0)
				{
					i += iAddCount;
				}
			}
		}

		using (var writer = XmlWriter.Create(xmlName, xws))
		{
			doc.Save(writer);
		}
	}
	static void ProcXml(object fileName)
	{
		try
		{
			HandleXml(fileName.ToString());
		}
		catch (Exception e)
		{
			Console.WriteLine(fileName.ToString());
			Console.WriteLine(e.Message);
		}
	}

	static int ParseArgs(string[] args)
	{
		if (args.Length < 1)
		{
			Console.WriteLine(@"参数错误");
			return -1;
		}

		workPath = args[0];

		return 0;
	}

	static void Main(string[] args)
	{
		int iRet = ParseArgs(args);
		if (iRet != 0)
		{
			return;
		}

		xws.Encoding = new UTF8Encoding(false);
		xws.Indent = true;
		xws.IndentChars = "\t";
		xws.NewLineHandling = NewLineHandling.None;

		DateTime beforDT = System.DateTime.Now;

		DirectoryInfo di = new DirectoryInfo(workPath);
		var files = di.GetFiles("*.xml");

		System.IO.Directory.SetCurrentDirectory(workPath);

 		foreach (var file in files)
 		{
			ProcXml(file.Name);
 		}

		DateTime afterDT = System.DateTime.Now;
		TimeSpan ts = afterDT.Subtract(beforDT);
		Console.WriteLine("Done: Used " + ts.Seconds.ToString() + "s");

		return;
	}
}
