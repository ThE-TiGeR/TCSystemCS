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
//  Copyright (C) 2003 - 2024 Thomas Goessler. All Rights Reserved.
// *******************************************************************************
// 
//  TCSystem is the legal property of its developers.
//  Please refer to the COPYRIGHT file distributed with this source distribution.
// 
// *******************************************************************************

#region Usings

using System.IO;
using NUnit.Framework;
using TCSystem.MetaData;

#endregion

namespace TCSystem.MetaDataDB.Tests
{
    public class DBSetup
    {
#region Public

        [SetUp]
        public void InitTestDB()
        {
            var dbFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _db = Factory.CreateReadWrite(dbFileName);
            _dbReadOnly = Factory.CreateRead(dbFileName);
        }

        [TearDown]
        public static void DeInitTestDB()
        {
            Factory.Destroy(ref _db);
            Factory.Destroy(ref _dbReadOnly);
            var dbFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.Delete(dbFileName);
        }

#endregion

#region Protected

        protected void AssertImageDataNotEqual(Image data1, Image data2)
        {
            data1 = data1.InvalidateId();
            data2 = data2.InvalidateId();

            Assert.That(data1.Tags, Is.EquivalentTo(data2.Tags));
            Assert.That(data1.PersonTags, Is.EquivalentTo(data2.PersonTags));
            Assert.That(data1.Location, Is.EqualTo(data2.Location));
            Assert.That(data1.DateTaken, Is.EqualTo(data2.DateTaken));
            Assert.That(data1.InvalidateId(), Is.EqualTo(data2.InvalidateId()));
        }

        protected IDB2 DB => _db;
        protected IDB2Read DBReadOnly => _dbReadOnly;

#endregion

#region Private

        private static IDB2 _db;
        private static IDB2Read _dbReadOnly;

#endregion
    }
}