using LittleUmph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
#if NET35_OR_GREATER
using System.Linq;
#endif

namespace LittleUmpTest
{
    
    
    /// <summary>
    ///This is a test class for EnmTest and is intended
    ///to contain all EnmTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnmTest
    {

        [Flags]
        enum EF
        {
            NONE = 0,
            A = 1,
            B = 2,
            C = 4,
            D = 8
        }

        enum E
        {
            A=1,B=2,C=4,D=8
        }

        EF ab = EF.A | EF.B;

        [TestMethod()]
        public void AddTest()
        {
            EF a = EF.A;
            EF acd = Enm.Add(a, EF.C | EF.D);

            Assert.AreEqual(acd, EF.A | EF.C | EF.D);

            EF result = Enm.Add(EF.NONE, ab);
            Assert.AreEqual(result, ab);
        }

        [TestMethod()]
        public void AddTestNonFlagEnum()
        {
            E resultAb = Enm.Add(E.A, E.B);
            Assert.IsFalse(Enm.GetList<E>().Contains(resultAb));
        }

        [TestMethod()]
        public void EachTest()
        {
            List<EF> list = new List<EF>();

            Enm.Each(ab, enm =>
            {
                list.Add(enm);
            });

            Assert.AreEqual(list.Count, 2);
            Assert.AreEqual(EF.A, list[0]);
            Assert.AreEqual(EF.B, list[1]);
        }


        [TestMethod()]
        public void FromIntTest()
        {
        }

        [TestMethod()]
        public void FromStringTest()
        {

        }

        [TestMethod()]
        public void HasFlagTest()
        {
        }

        [TestMethod()]
        public void RemoveTest()
        {
        }

        [TestMethod()]
        public void ToListTest()
        {
        }
    }
}
