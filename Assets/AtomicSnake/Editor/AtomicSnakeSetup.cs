using AtomicSnake.Chemistry;
using UnityEngine;

namespace AtomicSnake.EditorTools
{
    // Batch-mode helper: -executeMethod AtomicSnake.EditorTools.AtomicSnakeSetup.VerifyChemistry
    public static class AtomicSnakeSetup
    {
        public static void VerifyChemistry()
        {
            void Check(int atomicNumber, string expected)
            {
                string actual = IupacNaming.GetDisplayName(atomicNumber);
                if (actual != expected)
                    throw new System.Exception($"IupacNaming mismatch for {atomicNumber}: expected '{expected}', got '{actual}'");
                Debug.Log($"OK  {atomicNumber} -> {actual}");
            }

            Check(101, "Unnilunium");
            Check(102, "Unnilbium");
            Check(103, "Unniltrium");
            Check(104, "Unnilquadium");
            Check(108, "Unniloctium");
            Check(109, "Unnilennium");
            Check(110, "Ununnilium");
            Check(111, "Unununium");
            Check(112, "Ununbium");
            Check(113, "Ununtrium");
            Check(114, "Ununquadium");
            Check(118, "Ununoctium");

            Debug.Log("AtomicSnakeSetup.VerifyChemistry: all checks passed.");
        }
    }
}
