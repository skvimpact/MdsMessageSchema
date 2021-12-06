using MdsModelStorage;
using StringTools;
using System.Collections.Generic;
using XmlTools;

namespace MdsMessageSchema
{
    public partial class MessageSchema
    {
        public IEnumerable<ClassData> ModelClasses(string[] nameSpace) {
            var cd = new ClassData();
            string[] excluded = new string[] { "MessageID", "MessageDT", "CorrelationID", "SourceCode", "SourceAreaID", "DestinationCode", "Method" };
            foreach (var cl in XmlDocument.ModelClasses(
                nameSpace,
                path => {
                    var item = ItemWithXPath(path);
                    return (item.Type, item.IsArray);
                }))
            {
                if (cl.Root)
                {
                    foreach (var e in excluded)
                    {
                        cl.Content = cl.Content.RemoveAll(e);
                    }

                    yield return new ClassData()
                    {

                    Content = cl.Content
                        .Inject(InjectType.BeforeFirst,
                            "class",
                            $"\t[Soap(Codeunit = \"{codeunit}\", Method = \"{method}\", {nameof(Base64Option)} = {nameof(Base64Option)}.{base64Option})]")
                        .Inject(InjectType.AppendFirst,
                            "class",
                            " : Rq")
                        ,Name = cl.Name
                    };
                }
                else
                    yield return cl;

            }
        }
    }
}
