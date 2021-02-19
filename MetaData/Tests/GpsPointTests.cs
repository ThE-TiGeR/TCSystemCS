﻿// *******************************************************************************
// 
//  *******   ***   ***               *
//     *     *     *                  *
//     *    *      *                *****
//     *    *       ***  *   *   **   *    **    ***
//     *    *          *  * *   *     *   ****  * * *
//     *     *         *   *      *   * * *     * * *
//     *      ***   ***    *     **   **   **   *   *
//                         *
// *******************************************************************************
//  see https://github.com/ThE-TiGeR/TCSystemCS for details.
//  Copyright (C) 2003 - 2021 Thomas Goessler. All Rights Reserved.
// *******************************************************************************
// 
//  TCSystem is the legal property of its developers.
//  Please refer to the COPYRIGHT file distributed with this source distribution.
// 
// *******************************************************************************

#region Usings

using System;
using NUnit.Framework;

#endregion

namespace TCSystem.MetaData.Tests
{
    [TestFixture]
    public class GpsPointTests
    {
        [Test]
        public void EqualsTest()
        {
            var point = TestData.PointZero;
            Assert.That(point.Equals(TestData.PointZero), Is.True);
            Assert.That(point.Equals(TestData.Point1), Is.False);
            Assert.That(point.Equals(TestData.Point2), Is.False);
            Assert.That(point.Equals(null), Is.False);
            Assert.That(point, Is.Not.EqualTo(TestData.Address1));
        }

        [Test]
        public void FromJsonStringTest()
        {
            string ToJson(GpsPoint d) => d.ToJsonString();
            Func<string, GpsPoint> fromJson = GpsPoint.FromJsonString;

            TestUtil.FromJsonStringTest(TestData.Point1, ToJson, fromJson);
            TestUtil.FromJsonStringTest(TestData.Point2, ToJson, fromJson);
            TestUtil.FromJsonStringTest(TestData.PointZero, ToJson, fromJson);
        }

        [Test]
        public void GetHashCodeTest()
        {
            Assert.That(TestData.PointZero.GetHashCode(), Is.Not.EqualTo(TestData.Point1.GetHashCode()));

            var point = new GpsPoint(TestData.Point1.Latitude, TestData.Point1.Longitude, TestData.Point1.Altitude);
            Assert.That(point.GetHashCode(), Is.EqualTo(TestData.Point1.GetHashCode()));
        }

        [Test]
        public void ToStringTest()
        {
            Assert.That(TestData.PointZero.ToString(), Is.Not.EqualTo(""));
        }
    }
}