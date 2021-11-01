
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MdsMessageSchema
{
    public class ClassContent
    {
        public string Class { get; set; }
        public string Content { get; set; }
        public string Model { get; set; }
    }

    public partial class MessageSchema
    {
        public IEnumerable<ClassContent> ModelClasses() =>
            //string.Join("\n",
            ModelClass(xmlDocument.Root);

        public IEnumerable<ClassContent> ModelClass(XElement xElement)
        {
            var sb = new StringBuilder();
            var namespaceName = CapitalizeFirstLetter(xmlDocument.Root.Name.LocalName);
            var className = CapitalizeFirstLetter(xElement.Name.LocalName);
            sb.AppendLine("using System.Xml.Serialization;");
            sb.AppendLine($"namespace Model.CLI.{namespaceName}");
            sb.AppendLine("{");
            if (xElement.Equals(xmlDocument.Root))
                sb.AppendLine($"\t[XmlRoot(ElementName = \"{xElement.Name.LocalName}\")]");

            sb.AppendLine($"\tpublic class {className}");
            sb.AppendLine("\t{");
            foreach(var element in xElement.Elements())
            {
                var property = CapitalizeFirstLetter(element.Name.LocalName);
                var xpath = XXPath(element);
                var item = items.Where(i => XPath(i.IntMessageLineID) == xpath).FirstOrDefault();



                var array = new Multiplicity[] { Multiplicity.None_Many, Multiplicity.One_Many };
                var arrayNull = new Multiplicity[] { Multiplicity.None_Many, Multiplicity.None_One };
                var ar = item == null ? string.Empty :
                    
                    array.Contains(item.Multiplicity) ? "[]" : string.Empty;
                var pr = item == null ? string.Empty :

                    element.Elements().Count() != 0 ? property : item.LineType.ToString();

                var nl = item == null ? string.Empty :

                    arrayNull.Contains(item.Multiplicity) ? "?" : string.Empty;

                sb.AppendLine($"\t\t[XmlElement(\"{element.Name.LocalName}\")]");
                //sb.AppendLine($"\t\tpublic {pr}{nl}{ar} {property} {{ get; set; }}");
                sb.AppendLine($"\t\tpublic string {property} {{ get; set; }}");
                if (element.Elements().Count() != 0)
                    foreach (var i in ModelClass(element))
                        yield return i;
                    
            }
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            yield return new ClassContent() { 
                Class = className, 
                Content = sb.ToString(), 
                Model = CapitalizeFirstLetter(xmlDocument.Root.Name.LocalName) };
        }

        public string CapitalizeFirstLetter(string text) =>
            new string(new[] { char.ToUpper(text.First()) }.Concat(text.Skip(1)).ToArray());
    }
}
