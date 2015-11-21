﻿#region License
// Copyright (c) Newtonsoft. All Rights Reserved.
// License: https://raw.github.com/JamesNK/Newtonsoft.Json.Schema/master/LICENSE.md
#endregion

using System.Collections.Generic;
using Newtonsoft.Json.Schema.Infrastructure;
using NUnit.Framework;

namespace Newtonsoft.Json.Schema.Tests.Infrastructure
{
    [TestFixture]
    public class EnumHelpersTests
    {
        [Test]
        public void GetAllEnums()
        {
            List<JSchemaType> allEnums = EnumHelpers.GetAllEnums<JSchemaType>();

            CollectionAssert.Contains(allEnums, JSchemaType.None);
            CollectionAssert.Contains(allEnums, JSchemaType.Array);
            CollectionAssert.Contains(allEnums, JSchemaType.Array | JSchemaType.Object);
            CollectionAssert.Contains(allEnums, JSchemaType.Array | JSchemaType.Object | JSchemaType.Integer);
        }
    }
}