﻿#region License
// Copyright (c) Newtonsoft. All Rights Reserved.
// License: https://raw.github.com/JamesNK/Newtonsoft.Json.Schema/master/LICENSE.md
#endregion

using System;
using System.Diagnostics.CodeAnalysis;

namespace Newtonsoft.Json.Schema.Infrastructure
{
    internal static class ValidationUtils
    {
        public static void ArgumentNotNull([NotNull]object? value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
