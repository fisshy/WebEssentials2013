﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using MadsKristensen.EditorExtensions.JavaScript;

namespace MadsKristensen.EditorExtensions.CoffeeScript
{
    public class CoffeeLintCompiler : JsHintCompiler
    {
        private static readonly string _compilerPath = Path.Combine(WebEssentialsResourceDirectory, @"nodejs\tools\node_modules\coffeelint\bin\coffeelint");
        public new readonly static string ConfigFileName = "coffeelint.json";

        public override IEnumerable<string> SourceExtensions { get { return new[] { ".coffee", ".iced" }; } }
        public override string ServiceName { get { return "CoffeeLint"; } }
        protected override string CompilerPath { get { return _compilerPath; } }
        protected override Func<string, IEnumerable<CompilerError>> ParseErrors
        {
            get { return ParseErrorsWithXml; }
        }

        protected override string GetArguments(string sourceFileName, string targetFileName, string mapFileName)
        {
            GetOrCreateGlobalSettings(ConfigFileName); // Ensure that default settings exist

            return String.Format(CultureInfo.CurrentCulture, "--checkstyle \"{0}\""
                               , sourceFileName);
        }

        private IEnumerable<CompilerError> ParseErrorsWithXml(string error)
        {
            try
            {
                return XDocument.Parse(error).Descendants("file").Select(file =>
                {
                    var fileName = file.Attribute("name").Value;
                    return file.Descendants("error").Select(e => new CompilerError
                    {
                        FileName = fileName,
                        // Column number is currently unavailable in their compiler
                        Line = int.Parse(e.Attribute("line").Value, CultureInfo.InvariantCulture),
                        Message = ServiceName + ": " + e.Attribute("message").Value
                    });
                }).First();
            }
            catch
            {
                Logger.Log(ServiceName + " parse error: " + error);
                return new[] { new CompilerError() { Message = error } };
            }
        }
    }
}