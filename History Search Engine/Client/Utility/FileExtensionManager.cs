using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Service.File;
using System.IO;
using System.Reflection;
using FileExtensionContracts;

namespace Client.Utility
{
    public class FileExtensionManager
    {
        private static string DllPath { get; set; }
        private static List<IFileReader> Readers { get; set; }

        public static void LoadExtension()
        {
            if (Directory.Exists(DllPath))
            {
                string[] dllFileNames = null;
                dllFileNames = Directory.GetFiles(DllPath, "*.dll");
                Readers = new List<IFileReader>();

                ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
                foreach (string dllFile in dllFileNames)
                {
                    AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                    Assembly assembly = Assembly.Load(an);
                    assemblies.Add(assembly);
                }

                Type pluginType = typeof(IFileReader);
                ICollection<Type> pluginTypes = new List<Type>();
                foreach (Assembly assembly in assemblies)
                {
                    if (assembly != null)
                    {
                        Type[] types = assembly.GetTypes();

                        foreach (Type type in types)
                        {
                            if (type.IsInterface)
                            {
                                continue;
                            }
                            else
                            {
                                throw new TypeLoadException("Invalid assembly type");
                            }
                        }
                    }
                }

                foreach (Type type in pluginTypes)
                {
                    IFileReader extension = (IFileReader)Activator.CreateInstance(type);
                    Readers.Add(extension);
                }
            }
            else
            {
                throw new DirectoryNotFoundException("Extension dll path does not exists");
            }
        }
    }
}
