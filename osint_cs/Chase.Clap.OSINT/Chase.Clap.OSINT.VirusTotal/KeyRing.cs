using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Chase.Clap.OSINT.VirusTotal
{
    public sealed class KeyRing
    {
        private readonly string[] keys;
        private int index = 0;

        private int mutex = 0;

        public KeyRing(string[] keys)
        {
            if (keys == null || keys.Length < 1)
                throw new ArgumentException();

            this.keys = new string[keys.Length];
            for(int i=0;i<keys.Length;i++)
            {
                this.keys[i] = keys[i];
                if (string.IsNullOrEmpty(this.keys[i]))
                    throw new ArgumentException();
            }
        }

        public string Next
        {
            get
            {
                while (true)
                {
                    if (0 == Interlocked.Exchange(ref mutex, 1))
                    {
                        string item = keys[index];
                        index++;
                        if (index >= keys.Length)
                            index = 0;
                        Interlocked.Exchange(ref mutex, 0);
                        return item;
                    }
                }
            }
        }
    }
}
