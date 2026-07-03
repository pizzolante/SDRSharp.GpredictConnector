using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SDRSharp.GpredictConnector;

namespace UnitTestGpredictConnector
{
    [TestClass]
    public class Test_rigctrld_vfo_ptt_split
    {
        private Rigctrld class_under_test_ = null;

        [TestInitialize()]
        public void Initialize()
        {
            class_under_test_ = new Rigctrld();
        }

        #region VFO tests

        [TestMethod]
        public void GetVFO_v()
        {
            string result = class_under_test_.ExecCommand("v");
            Assert.AreEqual("VFOA\n", result);
        }

        [TestMethod]
        public void SetVFO_V_VFOA()
        {
            string result = class_under_test_.ExecCommand("V VFOA");
            Assert.AreEqual("RPRT 0\n", result);
        }

        [TestMethod]
        public void SetVFO_V_VFOB()
        {
            string result = class_under_test_.ExecCommand("V VFOB");
            Assert.AreEqual("RPRT 0\n", result);
        }

        [TestMethod]
        public void SetVFO_V_noargs()
        {
            // "V" alone without VFO argument
            string result = class_under_test_.ExecCommand("V");
            Assert.AreEqual("RPRT 0\n", result);
        }

        #endregion

        #region PTT tests

        [TestMethod]
        public void GetPTT_t()
        {
            string result = class_under_test_.ExecCommand("t");
            Assert.AreEqual("0\n", result);
        }

        [TestMethod]
        public void SetPTT_T_0()
        {
            string result = class_under_test_.ExecCommand("T 0");
            Assert.AreEqual("RPRT 0\n", result);
        }

        [TestMethod]
        public void SetPTT_T_1()
        {
            string result = class_under_test_.ExecCommand("T 1");
            Assert.AreEqual("RPRT 0\n", result);
        }

        [TestMethod]
        public void SetPTT_T_noargs()
        {
            string result = class_under_test_.ExecCommand("T");
            Assert.AreEqual("RPRT 0\n", result);
        }

        #endregion

        #region Split tests

        [TestMethod]
        public void GetSplit_s()
        {
            string result = class_under_test_.ExecCommand("s");
            Assert.AreEqual("0\nVFOA\n", result);
        }

        [TestMethod]
        public void SetSplit_S_0_VFOA()
        {
            string result = class_under_test_.ExecCommand("S 0 VFOA");
            Assert.AreEqual("RPRT 0\n", result);
        }

        [TestMethod]
        public void SetSplit_S_1_VFOB()
        {
            string result = class_under_test_.ExecCommand("S 1 VFOB");
            Assert.AreEqual("RPRT 0\n", result);
        }

        #endregion

        #region Info test

        [TestMethod]
        public void DumpInfo_underscore()
        {
            string result = class_under_test_.ExecCommand("_");
            Assert.AreEqual("SDR#/gpredict\nVFOA\n", result);
        }

        #endregion
    }
}
