using Microsoft.VisualStudio.TestTools.UnitTesting;
using ConsistentHashing;
using System;
using System.IO;

namespace ConsistentHashingTests
{
    [TestClass]
    public class ProgramTest
    {
        [TestMethod]
        public void TestMethod()
        {
            // Add 10 nodes
            for (int i = 0; i < 10; i++)
            {
                Program.AddServer();
            }

            Program.RemoveServer(6);

            // RING STATUS WITH 10 NODES (id : hash):
            /*
            9 : 0ADE7C2CF97F75D009975F4D720D1FA6C19F4897
            4 : 1B6453892473A467D07372D45EB05ABC2031647A
            1 : 356A192B7913B04C54574D18C28D46E6395428AB
            3 : 77DE68DAECD823BABBB58EDB1C8E14D7106E83BB
            7 : 902BA3CDA1883801594B6E1B452790CC53948FDA
            5 : AC3478D69A3C81FA62E60F5C3696165A4E5E6AC4 // SAVE HERE
            10 : B1D5781111D84F7B3FE45A0852E59758CD7A87E5 // SAVE HERE
            6 : C1DFD96EEA8CC2B62785275BCA38AC261256E278 // ¡REMOVED!
            2 : DA4B9237BACCCDF19C0760CAB7AEC4A8359010B0 // SAVE HERE
            8 : FE5DBBCEA5CE7E2988B8C69BCFDFDE8904AABC1F
             */

            Program.SendDataToServer(4, "hola"); // hash: 99800B85D3383E3A2FB45EB7D0066A4879A9DAD0

            Program.RemoveServer(5);

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                string result = Program.GetDataFromServer(8, "99800B85D3383E3A2FB45EB7D0066A4879A9DAD0");

                string consoleOut = sw.ToString().Trim();
                // Use second node because first node was deleted
                Assert.AreEqual("Getting data 99800B85D3383E3A2FB45EB7D0066A4879A9DAD0 from server B1D5781111D84F7B3FE45A0852E59758CD7A87E5", consoleOut);

                Assert.AreEqual(result, "hola");
            }

        }
    }
}
