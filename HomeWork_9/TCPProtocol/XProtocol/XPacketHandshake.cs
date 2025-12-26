using XProtocol.Serializator;

namespace XProtocol
{
    public class XPacketHandshake
    {
        [XField(1)]
        public string Username;

        [XField(2)]
        public string ColorHex;
    }
}
