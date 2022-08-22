using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OsrsOpen.Chase.Reference.Net
{
    public abstract class IpAddressRange
    {
        [DebuggerStepThrough]
        private protected IpAddressRange()
        { }

        [DebuggerStepThrough]
        public abstract bool IsInRange(IPAddress addr);

        [DebuggerStepThrough]
        public bool IsInRange(string ipAddr)
        {
            if (!string.IsNullOrEmpty(ipAddr))
            {
                IPAddress tmp;
                if (IPAddress.TryParse(ipAddr, out tmp))
                    return IsInRange(tmp);
            }
            return false;
        }

        [DebuggerStepThrough]
        public static IpAddressRange Create(IPAddress addr, byte cidr)
        {
            if (addr != null && cidr <= 128)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    if (cidr < 33)
                        return new CidrAddressRange(addr, cidr);
                }
                else if (addr.AddressFamily == AddressFamily.InterNetworkV6)
                    return new CidrAddressRange(addr, cidr);
            }
            return null;
        }

        [DebuggerStepThrough]
        public static IpAddressRange Create(IPAddress low, IPAddress high)
        {
            if (low != null && high != null)
            {
                if (low.AddressFamily == high.AddressFamily)
                {
                    if (low.AddressFamily == AddressFamily.InterNetwork || low.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        byte[] lows = low.GetAddressBytes();
                        byte[] highs = high.GetAddressBytes();

                        if (low.AddressFamily == AddressFamily.InterNetwork)
                        {
                            for (int i = 0; i < lows.Length; i++)
                            {
                                if (lows[i] > highs[i])
                                    return null;
                                if (lows[i] < highs[i]) //the first non-equal octet must be strictly lower than the high octet
                                    return new LowHighAddressRange(low, high);
                            }
                        }
                        else
                        {
                            ushort ll;
                            ushort hh;
                            for (int i = 0; i < lows.Length; i += 2)
                            {
                                ll = BitConverter.ToUInt16(lows, i);
                                hh = BitConverter.ToUInt16(highs, i);
                                if (ll > hh)
                                    return null;
                                if (ll < hh) //the first non-equal octet must be strictly lower than the high octet
                                    return new LowHighAddressRange(low, high);
                            }
                        }
                    }
                }
            }
            return null;
        }

        [DebuggerStepThrough]
        public static IpAddressRange Create(IEnumerable<IpAddressRange> ranges)
        {
            if (ranges != null)
            {
                List<IpAddressRange> tmp = new List<IpAddressRange>();
                foreach (IpAddressRange cur in ranges)
                {
                    if (cur != null)
                        tmp.Add(cur);
                }
                if (tmp.Count > 1)
                    return new MultiRangeRange(tmp.ToArray());
                if (tmp.Count == 1)
                    return tmp[0];
                return new EmptyAddressRange();
            }
            return null;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static ushort ToUInt16(byte[] addr, int start)
        {
            return (ushort)((addr[start + 1] << 8 | addr[start]) & 0x0FFFF);
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static uint ToUInt32(byte[] addr, int start)
        {
            return (uint)((addr[start + 3] << 24 | addr[start + 2] << 16 | addr[start + 1] << 8 | addr[start]) & 0x0FFFFFFFF);
        }
    }

    public sealed class SingleAddressRange : IpAddressRange
    {
        private readonly IPAddress address;

        [DebuggerStepThrough]
        internal SingleAddressRange(IPAddress address)
        {
            this.address = address;
        }

        [DebuggerStepThrough]
        public override bool IsInRange(IPAddress addr)
        {
            if (addr != null)
                return address.Equals(addr);
            return false;
        }
    }

    public sealed class EmptyAddressRange : IpAddressRange
    {
        [DebuggerStepThrough]
        internal EmptyAddressRange()
        { }

        [DebuggerStepThrough]
        public override bool IsInRange(IPAddress addr)
        {
            return false;
        }
    }

    public sealed class MultiRangeRange : IpAddressRange
    {
        private readonly IpAddressRange[] ranges;

        [DebuggerStepThrough]
        internal MultiRangeRange(IpAddressRange[] ranges)
        {
            this.ranges = ranges;
        }

        [DebuggerStepThrough]
        public override bool IsInRange(IPAddress addr)
        {
            if (addr != null)
            {
                for (int i = 0; i < ranges.Length; i++)
                {
                    if (ranges[i].IsInRange(addr))
                        return true;
                }
            }
            return false;
        }
    }

    public sealed class LowHighAddressRange : IpAddressRange
    {
        private readonly byte[] low;
        private readonly byte[] high;

        [DebuggerStepThrough]
        internal LowHighAddressRange(IPAddress low, IPAddress high)
        {
            this.low = low.GetAddressBytes();
            this.high = high.GetAddressBytes();
        }

        [DebuggerStepThrough]
        public override bool IsInRange(IPAddress addr)
        {
            if (addr != null)
            {
                if (low.Length == 4)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        byte[] b = addr.GetAddressBytes();
                        int i = 0;
                        byte l = low[i];
                        byte h = high[i];

                        //check any exact equalities needed for low==high in upper octets
                        while (l == h && i < low.Length)
                        {
                            if (b[i] != l)
                                return false;
                            i++;
                            l = low[i];
                            h = high[i];
                        }

                        if (i < b.Length) //the current octet must be strictly low<high
                        {
                            //ok, we have at least 1 octet in which low<high
                            if (b[i] > l && b[i] < h)
                                return true; //strictly between low and high in this octet
                            if (b[i] < l || b[i] > h)
                                return false; //outside of range

                            //must be exact match of high or low
                            if (b[i] == l) //follow the low side exact match
                                return MatchLow(b, i);
                            else //if b[i] == h  //follow the high side exact match
                                return MatchHigh(b, i);
                        }
                        else
                            return true;  //low == high == addr
                    }
                }
                else if (addr.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    //same as for v4, but using hextets (2-byte qibbles) for compare
                    byte[] b = addr.GetAddressBytes();
                    int i = 0;
                    ushort l = IpAddressRange.ToUInt16(low, i);
                    ushort h = IpAddressRange.ToUInt16(high, i);
                    ushort a = IpAddressRange.ToUInt16(b, i);

                    //check any exact equalities needed for low==high in upper octets
                    while (l == h && i < low.Length)
                    {
                        if (a != l)
                            return false;
                        i += 2;
                        l = IpAddressRange.ToUInt16(low, i);
                        h = IpAddressRange.ToUInt16(high, i);
                        a = IpAddressRange.ToUInt16(b, i);
                    }

                    if (i < b.Length) //the current octet must be strictly low<high
                    {
                        //ok, we have at least 1 octet in which low<high
                        if (a > l && a < h)
                            return true; //strictly between low and high in this octet
                        if (a < l || a > h)
                            return false; //outside of range

                        //must be exact match of high or low
                        if (a == l) //follow the low side exact match
                            return MatchLow6(b, i);
                        else //if b[i] == h  //follow the high side exact match
                            return MatchHigh6(b, i);
                    }
                    else
                        return true;  //low == high == addr
                }
            }
            return false;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchLow6(byte[] addr, int i)
        {
            ushort a, l;
            while (i < addr.Length)
            {
                a = IpAddressRange.ToUInt16(addr, i);
                l = IpAddressRange.ToUInt16(low, i);
                if (a > l)
                    return true; //strictly above low limit
                if (a < l)
                    return false; //strictly below low limit
                i += 2;
            }
            return true; //exact match of low
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchHigh6(byte[] addr, int i)
        {
            ushort a, h;
            while (i < addr.Length)
            {
                a = IpAddressRange.ToUInt16(addr, i);
                h = IpAddressRange.ToUInt16(high, i);
                if (a < h)
                    return true; //strictly below high limit
                if (a > h)
                    return false; //strictly above high limit
                i += 2;
            }
            return true; //exact match of low
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchLow(byte[] addr, int i)
        {
            while (i < addr.Length)
            {
                if (addr[i] > low[i])
                    return true; //strictly above low limit
                if (addr[i] < low[i])
                    return false; //strictly below low limit
                i++;
            }
            return true; //exact match of low
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchHigh(byte[] addr, int i)
        {
            while (i < addr.Length)
            {
                if (addr[i] < high[i])
                    return true; //strictly below high limit
                if (addr[i] > high[i])
                    return false; //strictly above high limit
                i++;
            }
            return true; //exact match of low
        }
    }

    public sealed class CidrAddressRange : IpAddressRange
    {
        private readonly uint[] range; //pre-masked source ip
        private readonly uint[] mask;

        [DebuggerStepThrough]
        internal CidrAddressRange(IPAddress addr, byte cidr)
        {
            byte[] tmp = addr.GetAddressBytes();
            byte[] mask = new byte[tmp.Length]; //all 0s
            if (tmp.Length == 4) //IPv4
            {
                if (cidr <= 32)
                {
                    MakeMask(cidr, mask);
                    this.range = FinalizeMask(tmp, mask);
                    this.mask = FinalizeMask(mask);
                }
            }
            else if (tmp.Length == 16) //IPv6
            {
                if (cidr <= 128)
                {
                    MakeMask(cidr, mask);
                    this.range = FinalizeMask(tmp, mask);
                    this.mask = FinalizeMask(mask);
                }
            }
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint[] FinalizeMask(byte[] mask)
        {
            if (mask.Length == 4)
                return new uint[] { IpAddressRange.ToUInt32(mask, 0) };

            return new uint[] { IpAddressRange.ToUInt32(mask, 0), IpAddressRange.ToUInt32(mask, 4), IpAddressRange.ToUInt32(mask, 8), IpAddressRange.ToUInt32(mask, 12) };
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint[] FinalizeMask(byte[] addr, byte[] mask)
        {
            for (int i = 0; i < addr.Length; i++)
            {
                addr[i] &= mask[i];
            }

            if (addr.Length == 4)
                return new uint[] { IpAddressRange.ToUInt32(addr, 0) };

            return new uint[] { IpAddressRange.ToUInt32(addr, 0), IpAddressRange.ToUInt32(addr, 4), IpAddressRange.ToUInt32(addr, 8), IpAddressRange.ToUInt32(addr, 12) };
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MakeMask(byte cidr, byte[] mask)
        {
            int i = 0;
            while (cidr >= 8)
            {
                mask[i] = 255; //all 1s
                cidr -= 8;
                i++;
            }
            if (cidr > 0)
            {
                int b = 0;
                while (cidr > 0)
                {
                    b = (b | 256) >> 1; //put a 1 in bit 9 and shift 1 right
                    cidr--;
                }
                mask[i] = (byte)b;
            }
        }

        [DebuggerStepThrough]
        public override bool IsInRange(IPAddress addr)
        {
            if (addr != null)
            {
                if (range.Length == 1) //uint
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                        return CheckBytes(addr.GetAddressBytes());
                }
                else //v6
                {
                    if (addr.AddressFamily == AddressFamily.InterNetworkV6)
                        return CheckBytes(addr.GetAddressBytes());
                }
            }
            return false;
        }

        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool CheckBytes(byte[] bytes)
        {
            for (int i = 0, j = 0; i < bytes.Length && j < range.Length; i += 4, j++)
            {
                if ((mask[j] & IpAddressRange.ToUInt32(bytes, i)) != range[j])
                    return false;
            }
            return true;
        }
    }
}