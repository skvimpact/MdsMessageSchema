using MdsModelStorage;

namespace MdsMessageSchema
{
    public partial class MessageSchema
    {
        private string codeunit;
        private string page;
        private string method;
        private Base64Option base64Option;
        public MessageSchema InCodeunit(string codeunit)
        {
            this.codeunit = codeunit;
            return this;
        }

        public MessageSchema InPage(string page)
        {
            this.page = page;
            return this;
        }

        public MessageSchema CallMethod(string method)
        {
            this.method = method;
            return this;
        }

        public MessageSchema WithTransformation(Base64Option base64Option)
        {
            this.base64Option = base64Option;
            return this;
        }

    }
}
