using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace HA4IoT.Extensions.Networking
{
    public sealed class WakeUpOnLan
    {
        public static void Wakeup(string macAddress)
        {
            //remove all non 0-9, A-F, a-f characters 
            macAddress = Regex.Replace(macAddress, @"[^0-9A-Fa-f]", "");

            if (macAddress.Length != 12)
            {
                throw new ArgumentException("Invalid MAC address. Try again!");
            }
            
            WOLUdpClient client = new WOLUdpClient();

            //255.255.255.255  i.e broadcast, port = 12287
            client.Client.Connect(new IPAddress(0xffffffff), 0x2fff); 
            if (client.IsClientInBrodcastMode())
            {
                int byteCount = 0;
                byte[] bytes = new byte[102];
                for (int trailer = 0; trailer < 6; trailer++)
                {
                    bytes[byteCount++] = 0xFF;
                }
                for (int macPackets = 0; macPackets < 16; macPackets++)
                {
                    int i = 0;
                    for (int macBytes = 0; macBytes < 6; macBytes++)
                    {
                        bytes[byteCount++] =
                        byte.Parse(macAddress.Substring(i, 2), NumberStyles.HexNumber);
                        i += 2;
                    }
                }

                int returnValue = client.Client.Send(bytes);   
            }
            else
            {
                throw new Exception("Remote client could not be set in broadcast mode!");
            }
        }
        
        public class WOLUdpClient : UdpClient
        {
            public WOLUdpClient() : base()
            {
            }
            
            public bool IsClientInBrodcastMode()
            {
                bool broadcast = false;
                if (this.Active)
                {
                    try
                    {
                        this.Client.SetSocketOption(SocketOptionLevel.Socket,
                                SocketOptionName.Broadcast, 0);
                        broadcast = true;
                    }
                    catch
                    {
                        broadcast = false;
                    }
                }
                return broadcast;
            }
        }

    }
}
