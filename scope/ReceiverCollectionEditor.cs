using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;

namespace DGScope.Receivers
{
    public class ReceiverCollectionEditor : CollectionEditor
    {
        private List<Type> types = new List<Type>();

        public ReceiverCollectionEditor(Type type) : base(type)
        {
            LoadReceivers();
        }

        private void LoadReceivers()
        {
            DirectoryInfo dir = new DirectoryInfo(Environment.CurrentDirectory);
            FileInfo[] dlls = dir.GetFiles("*.dll");
            foreach (var dll in dlls)
            {
                Assembly assembly = Assembly.LoadFrom(dll.FullName);
                foreach (Type type in assembly.GetTypes())
                {
                    if (typeof(Receiver).IsAssignableFrom(type))
                    {
                        types.Add(type);
                    }
                }
            }
        }


        protected override Type[] CreateNewItemTypes()
        {
            return types.ToArray();
        }
    }


}
