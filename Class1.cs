using MelonLoader

namespace aaaaaaa
{
    [RegisterTypeInIl2Cpp]
    internal class StaticInitTester : MonoBehaviour
    {
        static StaticInitTester()
        {
            Console.WriteLine("STATICALLY INITD");
        }

        public StaticInitTester(IntPtr popopo) : base(popopo) { }
    }
}