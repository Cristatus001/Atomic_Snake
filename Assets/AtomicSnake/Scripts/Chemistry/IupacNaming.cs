using System.Collections.Generic;
using System.Text;

namespace AtomicSnake.Chemistry
{
    // Temporary IUPAC systematic element names (1978 recommendation):
    // each digit of the atomic number maps to a root word, concatenated,
    // followed by the "-ium" ending. A root ending in "i" (bi, tri) drops
    // that "i" when it lands directly before "-ium" (e.g. tri+ium -> trium).
    public static class IupacNaming
    {
        static readonly string[] RootByDigit =
        {
            "nil", "un", "bi", "tri", "quad", "pent", "hex", "sept", "oct", "enn"
        };

        public const string Suffix = "ium";

        public static string RootForDigit(int digit) => RootByDigit[digit];

        // The ordered list of collectible word-parts for a given atomic number,
        // e.g. 103 -> ["un", "nil", "tri", "ium"].
        public static List<string> GetCollectSequence(int atomicNumber)
        {
            var digits = DigitsOf(atomicNumber);
            var sequence = new List<string>(digits.Count + 1);
            foreach (var digit in digits)
                sequence.Add(RootForDigit(digit));
            sequence.Add(Suffix);
            return sequence;
        }

        // The assembled, human-readable name, e.g. 103 -> "Unniltrium".
        public static string GetDisplayName(int atomicNumber)
        {
            var digits = DigitsOf(atomicNumber);
            var sb = new StringBuilder();
            for (int i = 0; i < digits.Count; i++)
            {
                string root = RootForDigit(digits[i]);
                bool isLastBeforeSuffix = i == digits.Count - 1;
                if (isLastBeforeSuffix && (root == "bi" || root == "tri"))
                    root = root.Substring(0, root.Length - 1);
                sb.Append(root);
            }
            sb.Append(Suffix);
            sb[0] = char.ToUpperInvariant(sb[0]);
            return sb.ToString();
        }

        static List<int> DigitsOf(int number)
        {
            string s = number.ToString();
            var digits = new List<int>(s.Length);
            foreach (char c in s)
                digits.Add(c - '0');
            return digits;
        }
    }
}
