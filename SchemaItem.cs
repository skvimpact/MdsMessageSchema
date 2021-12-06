using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace MdsMessageSchema
{
    public enum ActionType
    {
        Add,
        Remove
    }

    public enum Multiplicity
    {
        One,       // 1
        None_One,  // 0..1
        One_Many,  // 1..N
        None_Many  // 0..N        
    }

    public enum LineType
    {
        Node,
        Element,
        Attribute,
        Array,
        Object,
        Property
    }

    public enum DataFormat
    {
        Text,
        Integer,
        Decimal,
        Boolean,
        Date,
        DateTime,
        Time,
        GUID
    }

    public class SchemaItem
    {
        public int IntMessageID { get; set; }
        public int IntMessageLineID { get; set; }
        public string ElementName { get; set; }
        public LineType LineType { get; set; }
        public Multiplicity Multiplicity { get; set; }
        public int ParentElementID { get; set; }
        public string Value { get; set; }

        public int Indentation { get; set; }
        public string XPath { get; set; }
        
        public DataFormat DataFormat { get; set; }
        public int Length { get; set; }
        public string TypeInXsdLibrary { get; set; }        
        private string clrType =>
            DataFormat.Equals(DataFormat.Text) ? "string" :
            DataFormat.Equals(DataFormat.Integer) ? "int" :
            DataFormat.Equals(DataFormat.Decimal) ? "decimal" :
            DataFormat.Equals(DataFormat.Boolean) ? "bool" :
            DataFormat.Equals(DataFormat.Date) ? "DateTime" :
            DataFormat.Equals(DataFormat.DateTime) ? "DateTime" :
            DataFormat.Equals(DataFormat.Time) ? "DateTime" :
            DataFormat.Equals(DataFormat.GUID) ? "Guid" : string.Empty;
        public string Type =>
            new Multiplicity[] { Multiplicity.None_Many, Multiplicity.None_One }.Contains(Multiplicity) && clrType != "string" ? $"{clrType}?" : clrType;
        public bool IsArray => 
            new Multiplicity[] { Multiplicity.None_Many, Multiplicity.One_Many }.Contains(Multiplicity);
    }
}
