using System;
using MyBPT;
using MyBPT.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MyBptTest
{
    [TestClass]
    public class MyBptTest
    {
        [TestMethod]
        public void Test_GameSession_RemoveOffsetMin()
        {
            Assert.AreEqual(4,GameSession.RemoveOffsetMin(4));
            Assert.AreEqual(0,GameSession.RemoveOffsetMin(-4));
            Assert.AreNotEqual(-4,GameSession.RemoveOffsetMin(-4));
        }
        [TestMethod]
        public void Test_GameSession_RemoveOffsetMax()
        {
            Assert.AreEqual(4, GameSession.RemoveOffsetMax(4, 32));
            Assert.AreEqual(32, GameSession.RemoveOffsetMax(64, 32));
            Assert.AreNotEqual(64, GameSession.RemoveOffsetMax(64, 32));
        }
        [TestMethod]
        public void Test_GameSession_FindCenter()
        {
            Assert.AreEqual(new Microsoft.Xna.Framework.Vector2(16, 16), GameSession.FindCenter(32, 32));
            Assert.AreEqual(new Microsoft.Xna.Framework.Vector2(32, 16), GameSession.FindCenter(64, 32));
            Assert.AreNotEqual(new Microsoft.Xna.Framework.Vector2(32, 32), GameSession.FindCenter(32, 32));
        }
        [TestMethod]
        public void Test_IsoCalculator_Conversion()
        {
            IsoCalculator isoCalculator = new IsoCalculator();
            Assert.AreEqual(new Point(25, 15), isoCalculator.IsoTo2D(new Point(10, 10)));
            Assert.AreEqual(new Point(117, 71), isoCalculator.IsoTo2D(new Point(47, 47)));
            Assert.AreEqual(new Point(15, 57), isoCalculator.TwoDToIso(new Point(65, 50)));
            Assert.AreEqual(new Point(40, 180), isoCalculator.TwoDToIso(new Point(200, 160)));
        }
        [TestMethod]
        public void Test_Player_Name()
        {
            Player player = new Player("testperson");
            Assert.AreEqual("testperson", player.Name);
            player.Name = "testplayerwithanothername";
            Assert.AreEqual("testplayerwithanothername", player.Name);
            player = new Player("");
            Assert.AreNotEqual(null, player.Name);
        }
        [TestMethod]
        public void Test_Player_Money()
        {
            Player player = new Player("");
            player.AddMoney(500);
            Assert.AreEqual(500, player.Money);
            player.AddMoney(-200);
            Assert.AreEqual(300, player.Money);
            player.AddMoney(-5000);
            Assert.AreNotEqual(-4700, player.Money);
        }
        [TestMethod]
        public void Test_Player_UpdateLevel()
        {
            Player player = new Player("", 0, 1);
            player.UpdateLevel(1);
            Assert.AreNotEqual(2, player.Level);
            player.UpdateLevel(2);
            Assert.AreEqual(2, player.Level);
            player.UpdateLevel(-10);
            Assert.AreNotEqual(-10, player.Level);
        }
        [TestMethod]
        public void Test_Player_CanAfford()
        {
            Player player = new Player("", 500, 1);
            Assert.IsTrue(player.CanAfford(200));
            Assert.IsFalse(player.CanAfford(600));
            player.AddMoney(200);
            Assert.IsTrue(player.CanAfford(600));
        }
    }
}
