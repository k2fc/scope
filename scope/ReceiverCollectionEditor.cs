using System;
using System.ComponentModel.Design;

namespace DGScope.Receivers
{
    public class ReceiverCollectionEditor : CollectionEditor
    {
        private Type[] types;

        public ReceiverCollectionEditor(Type type) : base(type)
        {
            types = new Type[] { typeof(SBSReceiver)};
        }


        protected override Type[] CreateNewItemTypes()
        {
            return types;
        }
    }

    
}
