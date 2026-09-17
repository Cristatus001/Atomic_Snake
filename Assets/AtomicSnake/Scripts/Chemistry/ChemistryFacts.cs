using UnityEngine;

namespace AtomicSnake.Chemistry
{
    // Short trivia shown on the loading screen, per the GDD's
    // "loading screen... a fact about the topic".
    public static class ChemistryFacts
    {
        static readonly string[] Facts =
        {
            "The temporary system lets scientists refer to a brand-new element before its real name is agreed on.",
            "Every temporary element name ends in \"-ium\", no matter which digits come before it.",
            "Element 101 (Mendelevium) once carried the temporary name Unnilunium.",
            "The temporary naming system was recommended by IUPAC in 1978.",
            "\"Un\" stands for the digit 1 in every temporary element name.",
            "Elements 101 to 118 fill out the 7th row of the periodic table.",
            "A temporary name's abbreviation uses just the first letter of each digit root.",
            "Oganesson (118) was once known by the placeholder name Ununoctium.",
        };

        public static string GetRandom()
        {
            return Facts[Random.Range(0, Facts.Length)];
        }
    }
}
