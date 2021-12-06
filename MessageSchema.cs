using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using XmlTools;

namespace MdsMessageSchema
{
    public partial class MessageSchema
    {
        
        private readonly SchemaItem[] items;
        private static readonly string Wrap = "Wrap";
        private readonly XDocument xmlDocument;
        private readonly XDocument xsdDocument;
        public XDocument XmlDocument => xmlDocument;
        public XDocument XsdDocument => xsdDocument;
        //readonly Dictionary<int, XElement> xElements;
        //readonly Dictionary<int, XAttribute> xAttributes;
        public static MessageSchema Analyze(IEnumerable<SchemaItem> items) =>
            new MessageSchema(items);

        public SchemaItem ItemWithXPath(string xpath) => 
            items.Where(i => i.XPath == xpath).FirstOrDefault();
        

        private MessageSchema(IEnumerable<SchemaItem> items)
        {
            this.items = items.ToArray();
            (int rootId, string rootName) = items
                .Where(i => i.ParentElementID == 0)
                .Select(i => (i.IntMessageLineID, i.ElementName))
                .FirstOrDefault();

            xmlDocument = new XDocument(
                new XDeclaration("1.0", "utf-8", string.Empty),
                new XElement(rootName));

            Dictionary<int, XElement> xElements = new() { { rootId, xmlDocument.Root } };
            Dictionary<int, XAttribute> xAttributes = new();

            foreach (var item in items.Where(i => i.ParentElementID != 0))
            {
                object content = default(object);
                switch(item.DataFormat)
                {
                    case DataFormat.Text:
                        content = item.ElementName;// "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";//.Substring(0, item.Length <= 50 ? item.Length : 50);
                        break;
                    case DataFormat.Integer:
                        content = 255;
                        break;
                    case DataFormat.Decimal:
                        content = 34.047;
                        break;
                    case DataFormat.Boolean:
                        content = true;
                        break;
                    case DataFormat.DateTime:
                        content = DateTime.Now;
                        break;
                    case DataFormat.GUID:
                        content = Guid.NewGuid();
                        break;
                    case DataFormat.Date:
                        content = DateTime.Now;
                        break;
                    case DataFormat.Time:
                        content = DateTime.Now;
                        break;
                }

                switch (item.LineType)
                {
                    case LineType.Node:
                        xElements[item.IntMessageLineID] =
                            new XElement(item.ElementName);
                        break;
                    case LineType.Element:
                        xElements[item.IntMessageLineID] =
                            new XElement(item.ElementName, content);
                        break;
                    case LineType.Attribute:
                        xAttributes[item.IntMessageLineID] =
                            new XAttribute(item.ElementName, content);
                        break;
                    default:
                        throw new InvalidOperationException("unknown item type");
                }
            }

            foreach (var item in items.Where(i => i.ParentElementID != 0))
            {
                switch (item.LineType)
                {
                    case LineType.Node:
                    case LineType.Element:
                        xElements[item.ParentElementID]
                            .Add(xElements[item.IntMessageLineID]);
                        break;
                    case LineType.Attribute:
                        xElements[item.ParentElementID]
                            .Add(xAttributes[item.IntMessageLineID]);
                        break;
                    default:
                        throw new InvalidOperationException("unknown item type");
                }
                item.Indentation = Indentation(item.IntMessageLineID);
                item.XPath = XPath(item.IntMessageLineID);
            }

            xsdDocument = Xsd.Infer(All());
            excludeWrap();

            Dictionary<string, XElement> elementXPath = new Dictionary<string, XElement>();
            xsdDocument
                .Descendants()
                .Where(x => x.HasAttributes && x.Attribute("name") != null)
                .ToList()
                .ForEach(element => elementXPath[element.XsdXPath()] = element);

            items.ToList().ForEach(
                i => {
                    var element = elementXPath[XPath(i.IntMessageLineID)];
                   // if (element.Attribute("type") != null)
                   //     element.Attribute("type").Value = $"{i.IntMessageLineID}";
                   // TypeXsd
                        });

        }

        override public string ToString() => xmlDocument.ToString();

        public IEnumerable<string> Paths(int id)
        {
            var item = items.Where(i => i.IntMessageLineID.Equals(id)).FirstOrDefault();
            if (!item.ParentElementID.Equals(0))
                foreach(var itemName  in Paths(item.ParentElementID))                
                    yield return itemName;                
            yield return item.ElementName;            
        }

        public string XPath(int id) =>
            string.Join("/", Paths(id));

        public int Indentation(int id)
        {
            //SchemaItems.Where(i => i.IntMessageLineID.Equals(id)).FirstOrDefault().ParentElementID.Equals(0) ?
            //0 :
            //1 + Indentation(SchemaItems.Where(i => i.IntMessageLineID.Equals(id)).FirstOrDefault().ParentElementID);
            var parent = items.Where(i => i.IntMessageLineID.Equals(id)).First().ParentElementID;
            return parent.Equals(0) ? 0 : 1 + Indentation(parent);
        }

        public string DocumentVar(int id, ActionType actionType)
        {
            XDocument doc = XDocument.Load(new StringReader(xmlDocument.ToString()));
            var paths = Paths(id);

            if (paths.Count() == 1)
                return $"{doc}";

            XElement parent = doc.Element(paths.First());
            foreach (var name in paths.Skip(1).SkipLast(1))
                parent = parent.Element(name);

            XElement element = parent.Element(paths.Last());
            XAttribute attribute = parent.Attribute(paths.Last());

            switch (actionType)
            {
                case ActionType.Add :
                    for (int i = 0; i < 5; i++)
                        parent.Add(element);
                    break;
                case ActionType.Remove:                    
                    element?.Remove();
                    attribute?.Remove();
                    break;
            }
            return doc.ToString();
        }

        public IEnumerable<(int id, string name, ActionType actionType)> Deviations()
        {            
            foreach (var i in items)
                switch (i.Multiplicity)
                {
                    case Multiplicity.None_Many:
                        yield return (i.IntMessageLineID, i.ElementName, ActionType.Add);
                        yield return (i.IntMessageLineID, i.ElementName, ActionType.Remove);
                        break;
                    case Multiplicity.None_One:
                        yield return (i.IntMessageLineID, i.ElementName, ActionType.Remove);
                        break;
                    case Multiplicity.One_Many:
                        yield return (i.IntMessageLineID, i.ElementName, ActionType.Add);
                        break;
                }
        }

        public IEnumerable<string> AllVariants()
        {
            yield return $"<!-- Normal -->\n{xmlDocument}";
            foreach (var deviation in Deviations())
                yield return $"<!-- {deviation.id}/{deviation.name}/{deviation.actionType} -->\n" +
                    $"{DocumentVar(deviation.id, deviation.actionType)}";
        }

        public string All()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"<{Wrap}>");
            foreach (var variant in AllVariants())
            {
                sb.AppendLine(variant);
            }
            sb.AppendLine($"</{Wrap}>");
            return sb.ToString();
        }

        public MessageSchema IncludeSchema(string schema)
        {
            XNamespace xs = "http://www.w3.org/2001/XMLSchema";
            xsdDocument.Root.AddFirst(new XElement(xs + "include", new XAttribute("schemaLocation", schema)));
            return this;
        }

        private void excludeWrap()
        {
            //var xsdMarkup = XDocument.Load(new StringReader(xsdWrapped));

            XElement extracted = xsdDocument
                .Descendants()?
                .Where(i => i.Attribute("name")?.Value.Equals(xmlDocument.Root.Name.LocalName) ?? false)
                .FirstOrDefault();

            extracted?
                .Attribute("maxOccurs")?
                .Remove();

            xsdDocument
                .Descendants()
                .Where(i => i.Attribute("name")?.Value.Equals(Wrap) ?? false)
                .FirstOrDefault()?
                .Remove();

            xsdDocument
                .Root
                .Add(extracted);
        }

        public IEnumerable<string> ValidateAll()
        {
            string message;
            foreach(var variant in AllVariants())
            {
                message = "l";// XsdValidator.Validate(xsdDocument.ToString(), variant);
                if (!string.IsNullOrEmpty(message))
                    yield return message;
            }
        }
    }
}
