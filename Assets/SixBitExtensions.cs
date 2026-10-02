using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SixBitExtensionsNamespace
{
    public static class SixBitExtensions
    {
        public static char getChar(int index) =>
            "\tabcdefg0123456789.,!@#$ABCDEFGHIJKLMNOPQRSTUVWXYZ^&*-+=~?<>[]()"[index];

        public static int getInt(char c) =>
            "\tabcdefg0123456789.,!@#$ABCDEFGHIJKLMNOPQRSTUVWXYZ^&*-+=~?<>[]()".IndexOf(c);

        public static int mod(int a, int b) => b == 0 ? 0 : ((a % b) + b) % b;

        public static int fromBinary(string bin) =>
            bin.ToCharArray().AsEnumerable().Reverse().Select((x, i) => x == '1' ? 1 << i : 0).Sum();

        public static string binToOct(string bin)
        {
            List<bool> rBin = bin.ToCharArray().AsEnumerable().Reverse().Select(x => x == '1').ToList();
            List<char> ans = new List<char>();
            int m = 0;
            for (int i = 0; i < rBin.Count; i++)
            {
                if (rBin[i]) m += 1 << (i % 3);
                if (i % 3 == 2 || i == rBin.Count - 1)
                {
                    ans.Add("01234567"[m]);
                    m = 0;
                }
            }

            return ans.AsEnumerable().Reverse().Select(x => x.ToString()).Aggregate((a, b) => a + b);
        }

        public static string decToOct(int num, int octDigits)
        {
            List<char> ans = new List<char>();
            for (; octDigits > 0; octDigits--)
            {
                ans.Add("01234567"[num % 8]);
                num /= 8;
            }
            return ans.AsEnumerable().Reverse().Select(x => x.ToString()).Aggregate((a, b) => a + b);
        }

        public static string randomBinary(int length)
        {
            return Enumerable.Range(0, length).Select(_ => Random.value>.5f?"1":"0").Aggregate((a, b) => a + b);
        }
    }
}