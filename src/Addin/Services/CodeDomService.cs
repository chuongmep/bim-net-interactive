﻿
/*
 * Give credit to https://github.com/ricaun/RevitAddin.CodeCompileTest/blob/master/RevitAddin.CodeCompileTest/Services/CodeDomService.cs
 * */
using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;

namespace WpfBlazor.Services
{
        public interface ICodeCommand
        {
            public bool Execute(UIApplication uiapp);
        }
        public class CodeDomService
        {
    
            public Assembly CreateCommand(string commandCode)
            {
                string source = @"
            using Autodesk.Revit.Attributes;
            using Autodesk.Revit.DB;
            using Autodesk.Revit.UI;
            using System.Collections;
            using System.Linq;
            using System.Collections.Generic;
            using WpfBlazor.Services;

            namespace CodeNamespace 
            {
              public class Command : ICodeCommand
              {     
                public bool Execute(UIApplication uiapp)
                {
                    UIDocument uidoc = uiapp.ActiveUIDocument;"
+ commandCode + @"
                 return true;
                }
              }
            }";
                var targetUnit = new CodeSnippetCompileUnit(source);

                return GenerateCode(targetUnit);
                
            }
  

            public Assembly GenerateCode(params CodeCompileUnit[] compilationUnits)
            {
                CodeDomProvider provider = new CSharpCodeProvider();
                CompilerParameters compilerParametes = new CompilerParameters();

                compilerParametes.GenerateExecutable = false;
                compilerParametes.IncludeDebugInformation = false;
                compilerParametes.GenerateInMemory = false;

                #region Add GetReferencedAssemblies
                var assemblyNames = Assembly.GetExecutingAssembly().GetReferencedAssemblies().ToList();
                assemblyNames.Add(Assembly.GetExecutingAssembly().GetName());

                var nameAssemblies = new Dictionary<string, Assembly>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assemblyNames.Any(e => e.Name == assembly.GetName().Name))
                    {
                        nameAssemblies[assembly.GetName().Name] = assembly;
                    }
                }
                foreach (var keyAssembly in nameAssemblies)
                {
                    //Console.WriteLine($"Assembly: {keyAssembly.Key}");
                    compilerParametes.ReferencedAssemblies.Add(keyAssembly.Value.Location);
                }
                #endregion

                CompilerResults results = provider.CompileAssemblyFromDom(compilerParametes, compilationUnits);
                return results.CompiledAssembly;
            }
        }
}

