// Coders: Justin Yurick & Kiril Trenkov
// Date: April 11th, 2021

using System.Runtime.Serialization; // WCF data contract types

namespace BlackjackLibrary
{
    [DataContract]
    public class CallbackInfo
    {
        [DataMember]
        public string Message { get; private set; }

        public CallbackInfo(string m)
        {
            Message = m;
        }
    }
}