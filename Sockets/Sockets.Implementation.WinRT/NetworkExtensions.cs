using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sockets.Plugin
{
    /// <summary>
    /// Helper methods required for the conversion of platform-specific network items to the abstracted versions. 
    /// </summary>
    public static class NetworkExtensions
    {
        /// <summary>
        /// Determines the broadcast address for a given IPAddress
        /// Adapted from http://blogs.msdn.com/b/knom/archive/2008/12/31/ip-address-calculations-with-c-subnetmasks-networks.aspx
        /// </summary>
        /// <param name="address"></param>
        /// <param name="subnetMask"></param>
        /// <returns></returns>
        public static string GetBroadcastAddress(string address, string subnetMask)
        {
            var addressBytes = GetAddressBytes(address);
            var subnetBytes = GetAddressBytes(subnetMask);

            var broadcastBytes = addressBytes.Zip(subnetBytes, (a, s) => (byte)(a | (s ^ 255))).ToArray();

            return broadcastBytes.ToDottedQuadNotation();
        }

        /// <summary>
        /// Calculates the subnet address for an ip address given its prefix link.
        /// Returns the ip as a string in dotted quad notation.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="prefixLength"></param>
        /// <returns></returns>
        public static string GetSubnetAddress(string ipAddress, int prefixLength)
        {
            var maskBits = Enumerable.Range(0, 32)
                .Select(i => i < prefixLength)
                .ToArray();

            return maskBits
                .ToBytes()
                .ToDottedQuadNotation();
        }

        /// <summary>
        /// Converts an array of bools to an array of bytes, 8 bits per byte.
        /// Expects most significant bit first. 
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this bool[] bits)
        {
            var theseBits = bits.Reverse().ToArray();
            var ba = new BitArray(theseBits);

            var bytes = new byte[theseBits.Length/8];
            ((ICollection) ba).CopyTo(bytes,0);

            bytes = bytes.Reverse().ToArray();

            return bytes;
        }

        /// <summary>
        /// Converts dotted quad representation of an ip address into a byte array. 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static byte[] GetAddressBytes(string ipAddress)
        {
            var ipBytes = new byte[4];
            
            var parsesResults =
                ipAddress.Split('.')
                    .Select((p, i) => byte.TryParse(p, out ipBytes[i]))
                    .ToList();


            var valid = (parsesResults.Count == 4 && parsesResults.All(r => r));

            if (valid)
                return ipBytes;
            else
                throw new InvalidOperationException("The string provided did not appear to be a valid IP Address");
        }

        /// <summary>
        /// Converts a byte array into dotted quad representation.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToDottedQuadNotation(this byte[] bytes)
        {
            if (bytes.Length % 4 != 0)
                throw new InvalidOperationException("Byte array has an invalid byte count for dotted quad conversion");

            return String.Join(".", bytes.Select(b => ((int) b).ToString()));
        }
    }
}