using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;


[TestFixture]
public class MissileFunctionTesting
{
    Missile m;

    [TestFixtureSetUp]
    public void Init()
    {
        m = new Missile();
    }

    [Test]
    public void Test_That_Missile_is_Loading()
    {
        Assert.IsNotNull(m = new Missile());
    }

    [Test]
    public void Test_That_Missile_Can_Calculate_Targets_Velocity_and_Speed_Based_On_Two_Recorded_Position_Coords()
    {
        //While missile is in a stationary position, feed in two recorded positions of the target and test
        //that the missile can calculate the velocity of its target correctly.

        Vector3 targetVectorExpected = new Vector3(3, 0, 0);
        Vector3 actualTargetVelVector;
        TargetInfo test = new TargetInfo(new NetworkViewID(), new Vector3(0, 1, 0));

        m.OldTargetPosition = new Vector3(0, 1, 0); // old

        m.PrimaryTarget.TargetPosition = new Vector3(3, 1, 0); // new


        actualTargetVelVector = m.CalculateVelocityVector(m.OldTargetPosition, m.PrimaryTarget.TargetPosition, 1);

        Assert.AreEqual(targetVectorExpected, actualTargetVelVector);


    }

    [Test]
    public void Test_PredictIntercept()
    {
        Vector3 expected = new Vector3(3473.5f, 4305.5f, 1948.2f);

        Vector3 missilePositionMock = new Vector3(0, 0, 0); //   B


        TargetInfo test = new TargetInfo(new NetworkViewID(), new Vector3(150, 200, -300)); // A
        m.PrimaryTarget = test;

        m.TargetVelocityVector = new Vector3(34, 42, 23); // Av

        m.MaxSpeed = 60f;

        Vector3 intercept = m.CalculateInterceptVector(m.PrimaryTarget.TargetPosition, m.TargetVelocityVector, missilePositionMock, m.MaxSpeed);

        Console.WriteLine(intercept);

        Assert.AreEqual(expected.x, intercept.x, 0.1f);
        Assert.AreEqual(expected.y, intercept.y, 0.1f);
        Assert.AreEqual(expected.z, intercept.z, 0.1f);

    }

    [Test]
    public void Test_That_Missile_Is_Plotting_Course_Correctly()
    {
        //Test that the missile has the correct path and satisfies the fuel required demand before plotting its course.

        Vector3 expected = Vector3.Normalize(new Vector3(3473.5f, 4305.5f, 1948.2f)) * 60;

        Vector3 missilePositionMock = new Vector3(0, 0, 0); //   B


        TargetInfo test = new TargetInfo(new NetworkViewID(), new Vector3(150, 200, -300)); // A
        m.PrimaryTarget = test;

        m.TargetVelocityVector = new Vector3(34, 42, 23); // Av

        m.MaxSpeed = 60f;

        Vector3 intercept = m.CalculateInterceptVector(m.PrimaryTarget.TargetPosition, m.TargetVelocityVector, missilePositionMock, m.MaxSpeed);

        Vector3 plotInterceptVector = m.PlotCourse(intercept, missilePositionMock);

        Assert.AreEqual(expected.x, plotInterceptVector.x, 0.1f);
        Assert.AreEqual(expected.y, plotInterceptVector.y, 0.1f);
        Assert.AreEqual(expected.z, plotInterceptVector.z, 0.1f);
    }

    [Test]
    public void Test_If_Missile_Is_In_Detonation_Range()
    {
        // Test the missile is in close enough proximity that it can afford to detonate

        Vector3 missileMockPosition = new Vector3(0, 1, 0);

        TargetInfo test = new TargetInfo(new NetworkViewID(), new Vector3(14, 1, 0));

        m.PrimaryTarget = test;

        Assert.IsTrue(m.InDetonationRange(missileMockPosition,
            m.PrimaryTarget.TargetPosition), string.Format("{0}",
            Vector3.Distance(missileMockPosition,
            m.PrimaryTarget.TargetPosition)));

    }

    [Test]
    public void Test_If_Missile_Is_NOT_In_Detonation_Range()
    {
        // Test the missile is in close enough proximity that it can afford to detonate

        Vector3 missileMockPosition = new Vector3(0, 1, 0);

        TargetInfo test = new TargetInfo(new NetworkViewID(), new Vector3(129, 1, 0));

        m.PrimaryTarget = test;

        Assert.IsFalse(m.InDetonationRange(missileMockPosition,
            m.PrimaryTarget.TargetPosition),
            string.Format("{0}",
            Vector3.Distance(missileMockPosition,
            m.PrimaryTarget.TargetPosition)));

    }


}

