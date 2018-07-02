using System;

namespace TCP
{
    public delegate void ReceiveHandler(object sender, TCPReceiveEventArgs e);

    public class TCPReceiveEventArgs : EventArgs
    {
        public byte[] Bytes { get; set; }
        public string Message { get; set; }
        public string IP { get; private set; }

        public TCPReceiveEventArgs(string ip)
        {
            this.IP = ip;
        }

        public TCPReceiveEventArgs(byte[] bytes, string message, string ip)
        {
            this.Bytes = bytes;
            this.Message = message;
            this.IP = ip;
        }
    }
}