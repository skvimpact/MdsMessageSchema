using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace MdsMessageSchema
{
    /*
    public class XsdInferer
    {
        public static XDocument Infer(string xml)
        {
            XmlSchemaSet schemaSet = new XmlSchemaInference().InferSchema(XmlReader.Create(new StringReader(xml)));

            foreach (XmlSchema s in schemaSet.Schemas())            
                using (var stringWriter = new StringWriter())
                {
                    using (var xmlWriter = XmlWriter.Create(stringWriter))                    
                        s.Write(xmlWriter);                    
                    //return $"{stringWriter}";
                    return XDocument.Load(new StringReader(stringWriter.ToString()));
                }            
            return null;
        }

    }

    public class XsdValidator
    {
        public static string Validate(string xsdMarkup, string xsdDocument)
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add("", XmlReader.Create(new StringReader(xsdMarkup)));
            XDocument xdocument = XDocument.Load(new StringReader(xsdDocument.ToString()));
            //bool errors = false;
            string message = null;
            xdocument.Validate(schemas, (o, e) =>
            {
                //  Console.WriteLine("{0}", e.Message);
                // errors = true;
                message = e.Message;
            });
            return message;
        }       
    }
    */
}
