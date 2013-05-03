using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
class PlayerCraftFunctionTesting 
{
    PlayerCraft pc;

    [TestFixtureSetUp]
    public void Init()
    {
        pc = new PlayerCraft();        
    }

    [Test]
    public void TestPlayerCraftIsBeingCreated()
    {
        Assert.IsNotNull(pc);
    }

    [Test]
    public void TestThatTheMissileArrayIsBeingCreatedandContainsMissiles()
    {
        bool containsMissile = false;

        foreach (var missile in pc.MissileStock)
	    {
            if (missile is Missile)
            {
                containsMissile = true;
            }
            else
            {
                containsMissile = false;
            }
		}

        Assert.IsTrue(containsMissile);
	}   

    [Test]
    public void TestTheMissileStockNumber()
    {
        int defaultExpected = 4;

        Assert.AreEqual(defaultExpected, pc.MissileStock.Count());
    
    }

}

