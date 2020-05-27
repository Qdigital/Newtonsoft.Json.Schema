﻿#region License
// Copyright (c) Newtonsoft. All Rights Reserved.
// License: https://raw.github.com/JamesNK/Newtonsoft.Json.Schema/master/LICENSE.md
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Schema.Infrastructure;
#if !(NET20 || NET35 || PORTABLE) || DNXCORE50
using System.Numerics;
#endif
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Schema.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Schema.Tests
{
    [TestFixture]
    public class JSchemaValidatingReaderUnevaluatedTests : TestFixtureBase
    {
        [Test]
        public void UnevaluatedProperties_NotAllowed_NoMatch()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""unevaluatedProperties"": false
            }";

            string json = "{'bar':true}";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Boolean, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsNotNull(validationEventArgs);
            Assert.AreEqual("Property 'bar' has not been successfully evaluated and the schema does not allow unevaluated properties. Path '', line 1, position 12.", validationEventArgs.Message);
            Assert.AreEqual(ErrorType.UnevaluatedProperties, validationEventArgs.ValidationError.ErrorType);
        }

        [Test]
        public void UnevaluatedProperties_AnyOf_NoMatch()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""anyOf"": [
                    {
                        ""properties"": {
                            ""bar"": { ""const"": ""bar"" }
                        },
                        ""required"": [""bar""]
                    },
                    {
                        ""properties"": {
                            ""baz"": { ""const"": ""baz"" }
                        },
                        ""required"": [""baz""]
                    },
                    {
                        ""properties"": {
                            ""quux"": { ""const"": ""quux"" }
                        },
                        ""required"": [""quux""]
                    }
                ],
                ""unevaluatedProperties"": false
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar"",
                    ""baz"": ""not-baz""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNotNull(validationEventArgs);
            Assert.AreEqual("Property 'baz' has not been successfully evaluated and the schema does not allow unevaluated properties. Path '', line 5, position 17.", validationEventArgs.Message);
            Assert.AreEqual(ErrorType.UnevaluatedProperties, validationEventArgs.ValidationError.ErrorType);
        }

        [Test]
        public void UnevaluatedProperties_TrueWithUnevaluatedProperties_Match()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""unevaluatedProperties"": true
            }";

            string json = @"{
                    ""foo"": ""foo""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedProperties_DependentSchemas_NoUnevaluatedProperties()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""dependentSchemas"": {
                    ""foo"": {
                        ""properties"": {
                            ""bar"": { ""const"": ""bar"" }
                        },
                        ""required"": [""bar""]
                    }
                },
                ""unevaluatedProperties"": false
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedProperties_HasUnevaluatedProperty_AllowAllSchema()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""anyOf"": [
                    {
                        ""properties"": {
                            ""bar"": { ""const"": ""bar"" }
                        },
                        ""required"": [""bar""]
                    },
                    {
                        ""properties"": {
                            ""baz"": { ""const"": ""baz"" }
                        },
                        ""required"": [""baz""]
                    },
                    {
                        ""properties"": {
                            ""quux"": { ""const"": ""quux"" }
                        },
                        ""required"": [""quux""]
                    }
                ],
                ""unevaluatedProperties"": {}
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar"",
                    ""baz"": ""not-baz""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedProperties_HasUnevaluatedProperty_AdditionalPropertiesSchema()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""additionalProperties"": true,
                ""unevaluatedProperties"": false
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedProperties_HasUnevaluatedProperty_Not()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" },
                    ""baz"": { ""type"": ""string"" }
                },
                ""not"": {
                    ""not"": {
                        ""properties"": {
                            ""bar"": { ""const"": ""bar"" }
                        },
                        ""required"": [""bar""]
                    }
                },
                ""unevaluatedProperties"": false
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar"",
                    ""baz"": ""baz""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNotNull(validationEventArgs);
            Assert.AreEqual("Property 'bar' has not been successfully evaluated and the schema does not allow unevaluated properties. Path '', line 5, position 17.", validationEventArgs.Message);
            Assert.AreEqual(ErrorType.UnevaluatedProperties, validationEventArgs.ValidationError.ErrorType);
        }

        [Test]
        public void UnevaluatedProperties_NotAllowed_AllOfProperties_Match()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""allOf"": [
                    {
                        ""properties"": {
                            ""bar"": { ""type"": ""string"" }
                        }
                    }
                ],
                ""unevaluatedProperties"": false
            }";

            string json = "{'bar':'value'}";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedProperties_AnyOfProperties_Match()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""anyOf"": [
                    {
                        ""properties"": {
                            ""bar"": { ""const"": ""bar"" }
                        },
                        ""required"": [""bar""]
                    },
                    {
                        ""properties"": {
                            ""baz"": { ""const"": ""baz"" }
                        },
                        ""required"": [""baz""]
                    },
                    {
                        ""properties"": {
                            ""quux"": { ""const"": ""quux"" }
                        },
                        ""required"": [""quux""]
                    }
                ],
                ""unevaluatedProperties"": false
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedProperties_NestedAdditionalProperties_Match()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""allOf"": [
                    {
                        ""additionalProperties"": true
                    }
                ],
                ""unevaluatedProperties"": false
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedProperties_NestedUnevaluatedProperties_Match()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""allOf"": [
                    {
                        ""unevaluatedProperties"": true
                    }
                ],
                ""unevaluatedProperties"": {
                    ""type"": ""string"",
                    ""maxLength"": 2
                }
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedProperties_NestedUnevaluatedPropertiesSchema_Match()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""allOf"": [
                    {
                        ""unevaluatedProperties"": { ""type"": ""string"" }
                    }
                ],
                ""unevaluatedProperties"": {
                    ""type"": ""string"",
                    ""maxLength"": 2
                }
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedProperties_Schema_Match()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""unevaluatedProperties"": {
                    ""type"": ""string"",
                    ""minLength"": 3
                }
            }";

            string json = @"{
                    ""foo"": ""foo""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedItems_Schema_Match()
        {
            string schemaJson = @"{
                ""type"": ""array"",
                ""unevaluatedItems"": {
                    ""type"": ""string"",
                    ""minLength"": 3
                }
            }";

            string json = @"[ ""foo"" ]";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedItems_Schema_NoMatch()
        {
            string schemaJson = @"{
                ""type"": ""array"",
                ""unevaluatedItems"": {
                    ""type"": ""string"",
                    ""minLength"": 3
                }
            }";

            string json = @"[ ""fo"" ]";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNotNull(validationEventArgs);
            Assert.AreEqual("Item at index 0 has not been successfully evaluated and the schema does not allow unevaluated items. Path '', line 1, position 8.", validationEventArgs.Message);
            Assert.AreEqual(ErrorType.UnevaluatedItems, validationEventArgs.ValidationError.ErrorType);
        }

        [Test]
        public void UnevaluatedProperties_WithRef_Match()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""$ref"": ""#/$defs/bar"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""unevaluatedProperties"": false,
                ""$defs"": {
                    ""bar"": {
                        ""properties"": {
                            ""bar"": { ""type"": ""string"" }
                        }
                    }
                }
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }

        [Test]
        public void UnevaluatedProperties_WithRefAndAllOf_Match()
        {
            string schemaJson = @"{
                ""type"": ""object"",
                ""$ref"": ""#/$defs/bar"",
                ""properties"": {
                    ""foo"": { ""type"": ""string"" }
                },
                ""unevaluatedProperties"": false,
                ""$defs"": {
                    ""bar"": {
                        ""allOf"": [
                            {
                                ""properties"": {
                                    ""bar"": { ""type"": ""string"" }
                                }
                            }
                        ]
                    }
                }
            }";

            string json = @"{
                    ""foo"": ""foo"",
                    ""bar"": ""bar""
                }";

            SchemaValidationEventArgs validationEventArgs = null;

            JSchemaValidatingReader reader = new JSchemaValidatingReader(new JsonTextReader(new StringReader(json)));
            reader.ValidationEventHandler += (sender, args) => { validationEventArgs = args; };
            reader.Schema = JSchema.Parse(schemaJson);

            while (reader.Read())
            {
            }

            Assert.IsNull(validationEventArgs);
        }
    }
}